using Microsoft.Extensions.Options;
using NetXP.Exceptions;
using NetXP.Tts;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Web;

namespace NetXP.Tts.ElevenLabs
{
    public class TtsEvenLabs : ITts
    {
        private readonly IOptions<TtsElevenlabsOptions> options;
        public readonly HttpClient client;
        private string previousText;

        public TtsEvenLabs(IOptions<TtsElevenlabsOptions> options, IHttpClientFactory clientFactory)
        {
            this.options = options;

            this.client = clientFactory.CreateClient();
            this.client.BaseAddress = new Uri(options.Value.URL);
            this.client.DefaultRequestHeaders.Add("accept", "application/json");
            this.client.DefaultRequestHeaders.Add("xi-api-key", $"{this.options.Value.APIKey}");
        }

        public Task<TtsAudio> Convert(TtsConvertOption ttsConvertOption)
        {
            return Convert(ttsConvertOption, CancellationToken.None);
        }

        public async Task<TtsAudio> Convert(TtsConvertOption ttsConvertOption, CancellationToken token)
        {
            if (ttsConvertOption.Voice == null)
            {
                throw new CustomApplicationException("Voice cannot be null in TTSConvertOption");
            }

            var body = new
            {
                text = ttsConvertOption.Text,
                model_id = ttsConvertOption.Voice.ModelId,
                voice_settings = new
                {
                    stability = 0.3,
                    similarity_boost = 0.75,
                    style = 0,
                    use_speaker_boost = true,
                    speed = 0.95
                },
                previous_text = previousText,
                next_text = ttsConvertOption.NextText,
            };

            previousText = ttsConvertOption.Text;

            string json = JsonConvert.SerializeObject(body);
            StringContent httpContent = new(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(
                string.Format(this.options.Value.TextToSpeechUri, ttsConvertOption.Voice.Id),
                httpContent,
                token);

            token.ThrowIfCancellationRequested();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"{await response.Content.ReadAsStringAsync(token)}");
            }

            var audioToReturn = new TtsAudio()
            {
                Format = response?.Content?.Headers?.ContentType?.MediaType,
                File = new MemoryStream()
            };
            
            var streamResponse = await response.Content.ReadAsStreamAsync(token);
            await streamResponse.CopyToAsync(audioToReturn.File, token);

            return audioToReturn;
        }

        public Task<List<TtsVoice>> GetTtsVoices(string language = null)
        {
            return GetTtsVoices(language, CancellationToken.None);
        }

        public async Task<List<TtsVoice>> GetTtsVoices(string language, CancellationToken token)
        {
            var finalUrl = this.options.Value.GetVoicesUri;

            HttpResponseMessage response = await client.GetAsync(finalUrl, token);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error when trying to get voice {await response.Content.ReadAsStringAsync(token)}");
            }

            token.ThrowIfCancellationRequested();

            var result = await response.Content.ReadFromJsonAsync<Root>(cancellationToken: token);

            var ttsVoices = new List<TtsVoice>();
            foreach (var v in result.voices)
            {
                ttsVoices.Add(new TtsVoice()
                {
                    Id = v.voice_id,
                    Gender = v.labels.gender,
                    Language = v.fine_tuning?.language?.ToString(),
                    Name = v.name,
                    ModelId = v.high_quality_base_model_ids.FirstOrDefault(),
                    Tags = $"{v.category} {v.labels.accent} {v.labels.use_case} {v.labels.age} {v.labels.description}",
                });
            }

            return ttsVoices;
        }
    }
}