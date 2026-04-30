using Microsoft.Extensions.Options;
using NetXP.Exceptions;
using NetXP.Tts;
using System.Net.Http.Json;
using System.Text.Json;

namespace NetXP.Tts.ChatterboxApi
{
    public sealed class TtsChatterboxApi : ITts
    {
        private readonly IOptions<TtsChatterboxApiOptions> options;
        private readonly HttpClient client;

        private ChatterboxHealthResponse? cachedHealth;
        private DateTimeOffset healthCheckedAt = DateTimeOffset.MinValue;
        private Dictionary<string, string>? cachedLanguages;

        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public TtsChatterboxApi(IOptions<TtsChatterboxApiOptions> options, IHttpClientFactory clientFactory)
        {
            this.options = options;

            var baseUrl = options.Value.URL?.Trim();
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new CustomApplicationException("ChatterboxApiOptions.URL is required.");
            }

            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            client = clientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            client.DefaultRequestHeaders.Add("accept", "application/json");
        }

        public Task<TtsAudio> Convert(TtsConvertOption ttsConvertOption)
        {
            return Convert(ttsConvertOption, CancellationToken.None);
        }

        public async Task<TtsAudio> Convert(TtsConvertOption ttsConvertOption, CancellationToken token)
        {
            if (ttsConvertOption == null)
            {
                throw new CustomApplicationException("TtsConvertOption cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(ttsConvertOption.Text))
            {
                throw new CustomApplicationException("Text cannot be null or empty.");
            }

            await EnsureHealthAsync(token);

            var model = NormalizeModel(ttsConvertOption.Voice?.ModelId, options.Value.DefaultModel);
            var device = ResolveDevice();

            string? languageId = null;
            if (model == "multilingual")
            {
                var requestedLanguage = ttsConvertOption.Voice?.Language ?? options.Value.DefaultLanguage;
                languageId = await ResolveLanguageAsync(requestedLanguage, token);
                if (string.IsNullOrWhiteSpace(languageId))
                {
                    throw new CustomApplicationException("language_id is required when model is multilingual.");
                }
            }

            var voiceId = NormalizeVoiceId(ttsConvertOption.Voice?.Id);
            var request = new ChatterboxGenerateRequest
            {
                model = model,
                device = device,
                text = ttsConvertOption.Text,
                voice_id = voiceId,
                language_id = languageId,
                temperature = options.Value.Temperature,
                top_p = options.Value.TopP
            };

            using var response = await client.PostAsJsonAsync(options.Value.GenerateUri, request, Json, token);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(token);
                throw new CustomApplicationException($"Chatterbox generate failed: {error}");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(token);
            if (bytes.Length == 0)
            {
                throw new CustomApplicationException("Chatterbox returned empty audio response.");
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType ?? "audio/wav";

            return new TtsAudio
            {
                Format = mediaType,
                File = new MemoryStream(bytes, writable: false)
            };
        }

        public Task<List<TtsVoice>> GetTtsVoices(string? language = null)
        {
            return GetTtsVoices(language, CancellationToken.None);
        }

        public async Task<List<TtsVoice>> GetTtsVoices(string? language, CancellationToken token)
        {
            await EnsureHealthAsync(token);

            var model = NormalizeModel(null, options.Value.DefaultModel);
            string? resolvedLanguage = null;
            if (model == "multilingual")
            {
                resolvedLanguage = await ResolveLanguageAsync(language ?? options.Value.DefaultLanguage, token);
            }

            using var response = await client.GetAsync(options.Value.VoicesUri, token);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(token);
                throw new CustomApplicationException($"Chatterbox voices failed: {error}");
            }

            var payload = await response.Content.ReadFromJsonAsync<ChatterboxVoicesResponse>(Json, token)
                         ?? new ChatterboxVoicesResponse();

            var voices = new List<TtsVoice>
            {
                new()
                {
                    Id = "default",
                    Name = "Default model voice",
                    ModelId = model,
                    Gender = "N/A",
                    Language = resolvedLanguage,
                    Tags = "default"
                }
            };

            foreach (var voice in payload.voices)
            {
                var fileName = voice.filename ?? voice.id ?? "voice";
                voices.Add(new TtsVoice
                {
                    Id = voice.id,
                    Name = Path.GetFileNameWithoutExtension(fileName),
                    ModelId = model,
                    Gender = "N/A",
                    Language = resolvedLanguage,
                    Tags = voice.relative_path
                });
            }

            return voices;
        }

        private async Task EnsureHealthAsync(CancellationToken token)
        {
            if (!options.Value.UseHealthCheck)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            if (cachedHealth != null && now - healthCheckedAt < TimeSpan.FromSeconds(20))
            {
                return;
            }

            using var response = await client.GetAsync(options.Value.HealthUri, token);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(token);
                throw new CustomApplicationException($"Chatterbox health failed: {error}");
            }

            var health = await response.Content.ReadFromJsonAsync<ChatterboxHealthResponse>(Json, token);
            if (health == null || !string.Equals(health.status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                throw new CustomApplicationException("Chatterbox health check returned invalid status.");
            }

            cachedHealth = health;
            healthCheckedAt = now;
        }

        private string ResolveDevice()
        {
            var requested = (options.Value.DefaultDevice ?? "cuda").Trim().ToLowerInvariant();
            if (requested != "cpu" && requested != "cuda")
            {
                requested = "cuda";
            }

            if (requested == "cuda" && cachedHealth?.cuda_available == false)
            {
                return "cpu";
            }

            return requested;
        }

        private async Task<string?> ResolveLanguageAsync(string? requestedLanguage, CancellationToken token)
        {
            var languages = await GetLanguagesAsync(token);

            var normalized = NormalizeLanguageCode(requestedLanguage);
            if (!string.IsNullOrWhiteSpace(normalized) && languages.ContainsKey(normalized))
            {
                return normalized;
            }

            var fallback = NormalizeLanguageCode(options.Value.DefaultLanguage);
            if (!string.IsNullOrWhiteSpace(fallback) && languages.ContainsKey(fallback))
            {
                return fallback;
            }

            if (languages.ContainsKey("en"))
            {
                return "en";
            }

            return languages.Keys.FirstOrDefault();
        }

        private async Task<Dictionary<string, string>> GetLanguagesAsync(CancellationToken token)
        {
            if (cachedLanguages != null && cachedLanguages.Count > 0)
            {
                return cachedLanguages;
            }

            using var response = await client.GetAsync(options.Value.LanguagesUri, token);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(token);
                throw new CustomApplicationException($"Chatterbox languages failed: {error}");
            }

            var payload = await response.Content.ReadFromJsonAsync<ChatterboxLanguagesResponse>(Json, token)
                         ?? new ChatterboxLanguagesResponse();

            if (payload.languages.Count == 0)
            {
                throw new CustomApplicationException("Chatterbox returned no languages.");
            }

            cachedLanguages = payload.languages;
            return cachedLanguages;
        }

        private static string NormalizeModel(string? modelId, string? fallbackModel)
        {
            var value = (modelId ?? fallbackModel ?? "turbo").Trim().ToLowerInvariant();
            if (value.Contains("multi"))
            {
                return "multilingual";
            }

            return "turbo";
        }

        private static string? NormalizeVoiceId(string? voiceId)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                return null;
            }

            var normalized = voiceId.Trim();
            if (string.Equals(normalized, "default", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return normalized;
        }

        private static string? NormalizeLanguageCode(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var code = value.Trim().Replace("_", "-").ToLowerInvariant();
            if (code.Contains('-'))
            {
                code = code.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];
            }

            return code;
        }
    }
}
