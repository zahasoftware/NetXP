using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetXP.Exceptions;
using NetXP.IAs.ImageGeneratorAI;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace NetXP.ImageGeneratorAI.ComfyBridgeApi
{
    public sealed class ComfyBridgeVideoGeneratorClient : IVideoGeneratorAI
    {
        private static readonly string[] ImageFields = ["image", "file", "sourceImage", "startImage"];
        private static readonly string[] PromptFields = ["prompt", "text", "positivePrompt", "positive"];
        private static readonly string[] WidthFields = ["width", "imageWidth", "videoWidth"];
        private static readonly string[] HeightFields = ["height", "imageHeight", "videoHeight"];
        private static readonly string[] FpsFields = ["fps", "frameRate"];
        private static readonly string[] FrameCountFields = ["frameCount", "length", "frames", "numFrames"];
        private static readonly string[] NegativePromptFields = ["negativePrompt", "negative_prompt", "negative"];

        private readonly HttpClient _httpClient;
        private readonly ComfyBridgeClientOptions _clientOptions;
        private readonly ILogger<ComfyBridgeVideoGeneratorClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ComfyBridgeVideoGeneratorClient(
            HttpClient httpClient,
            IOptions<ComfyBridgeClientOptions> clientOptions,
            ILogger<ComfyBridgeVideoGeneratorClient> logger)
        {
            _httpClient = httpClient;
            _clientOptions = clientOptions?.Value ?? throw new ArgumentNullException(nameof(clientOptions));
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            };

            if (string.IsNullOrWhiteSpace(_clientOptions.BaseUrl))
            {
                throw new CustomApplicationException($"{ComfyBridgeClientOptions.SectionName}:BaseUrl is required.");
            }
        }

        public async Task<VideoGenerated> GenerateVideoAsync(VideoGenerationRequest request, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var comfyFileName = await ResolveComfyFileName(request, token);

            var templates = await GetTemplatesInternal(token);
            var selectedTemplate = SelectTemplate(templates);
            var payload = BuildPayload(selectedTemplate, request, comfyFileName);

            var endpoint = $"/api/v1/{selectedTemplate.Category}/{selectedTemplate.Name}";
            using var runResponse = await _httpClient.PostAsJsonAsync(endpoint, payload, _jsonOptions, token);
            var runJson = await ReadJsonOrThrow(runResponse, endpoint, token);

            var jobId = GetString(runJson, ["jobId", "JobId", "id", "Id"]);
            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new CustomApplicationException("ComfyBridge did not return a valid JobId for video generation.");
            }

            var completedJob = await PollJobUntilComplete(jobId, token);
            var assetUrls = ExtractAssetUrls(completedJob);
            if (assetUrls.Count == 0)
            {
                throw new CustomApplicationException($"ComfyBridge job '{jobId}' completed without downloadable assets.");
            }

            var selectedIndex = SelectBestVideoAssetIndex(assetUrls);
            var downloadEndpoint = $"/api/v1/jobs/{jobId}/outputs/{selectedIndex}";
            using var binaryResponse = await _httpClient.GetAsync(downloadEndpoint, token);
            await EnsureSuccess(binaryResponse, downloadEndpoint, token);

            var videoBytes = await binaryResponse.Content.ReadAsByteArrayAsync(token);
            return new VideoGenerated
            {
                Id = jobId,
                Url = assetUrls[selectedIndex] ?? string.Empty,
                Video = videoBytes
            };
        }

        private async Task<string> ResolveComfyFileName(VideoGenerationRequest request, CancellationToken token)
        {
            if (!string.IsNullOrWhiteSpace(request.SourceImagePath))
            {
                if (!File.Exists(request.SourceImagePath))
                {
                    throw new FileNotFoundException($"Source image path not found: {request.SourceImagePath}");
                }

                using var form = new MultipartFormDataContent();
                await using var stream = File.OpenRead(request.SourceImagePath);
                using var content = new StreamContent(stream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                form.Add(content, "file", Path.GetFileName(request.SourceImagePath));
                form.Add(new StringContent("input"), "type");
                form.Add(new StringContent("true"), "overwrite");

                const string endpoint = "/api/v1/uploads/image";
                using var uploadResponse = await _httpClient.PostAsync(endpoint, form, token);
                var uploadJson = await ReadJsonOrThrow(uploadResponse, endpoint, token);
                var uploadedFileName = GetString(uploadJson, ["comfyFileName", "ComfyFileName", "name", "filename"]);
                if (string.IsNullOrWhiteSpace(uploadedFileName))
                {
                    throw new CustomApplicationException("ComfyBridge upload did not return comfyFileName.");
                }

                return uploadedFileName;
            }

            if (!string.IsNullOrWhiteSpace(request.SourceImageId))
            {
                return request.SourceImageId;
            }

            throw new ArgumentException("VideoGenerationRequest requires SourceImagePath or SourceImageId.", nameof(request));
        }

        private async Task<JsonElement> PollJobUntilComplete(string jobId, CancellationToken token)
        {
            var timeout = _clientOptions.GetTimeout();
            var pollDelay = _clientOptions.GetPollInterval();
            var maxPollDelay = TimeSpan.FromSeconds(5);
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                var endpoint = $"/api/v1/jobs/{jobId}";
                using var pollResponse = await _httpClient.GetAsync(endpoint, token);
                var pollJson = await ReadJsonOrThrow(pollResponse, endpoint, token);
                var status = GetString(pollJson, ["status", "Status", "jobStatus"]);

                if (!string.IsNullOrWhiteSpace(status))
                {
                    if (IsCompletedStatus(status))
                    {
                        return pollJson;
                    }

                    if (IsFailedStatus(status))
                    {
                        var error = GetString(pollJson, ["errorMessage", "ErrorMessage", "error", "message"]);
                        throw new ComfyBridgeJobFailedException(jobId, error);
                    }
                }

                await Task.Delay(pollDelay, token);
                var nextDelayMs = Math.Min(pollDelay.TotalMilliseconds * 1.5d, maxPollDelay.TotalMilliseconds);
                pollDelay = TimeSpan.FromMilliseconds(nextDelayMs);
            }

            throw new TimeoutException($"ComfyBridge video job '{jobId}' did not finish before timeout ({timeout.TotalSeconds}s).");
        }

        private async Task<List<TemplateDescriptor>> GetTemplatesInternal(CancellationToken token)
        {
            const string endpoint = "/api/v1/templates";
            using var response = await _httpClient.GetAsync(endpoint, token);
            var root = await ReadJsonOrThrow(response, endpoint, token);

            var templates = ParseTemplates(root).ToList();
            if (templates.Count == 0)
            {
                throw new CustomApplicationException("ComfyBridge did not return templates.");
            }

            return templates;
        }

        private TemplateDescriptor SelectTemplate(IEnumerable<TemplateDescriptor> templates)
        {
            var templateList = templates.ToList();

            if (!string.IsNullOrWhiteSpace(_clientOptions.VideoTemplateName))
            {
                var explicitTemplate = templateList.FirstOrDefault(t =>
                    string.Equals(t.Name, _clientOptions.VideoTemplateName, StringComparison.OrdinalIgnoreCase)
                    && (string.IsNullOrWhiteSpace(_clientOptions.VideoCategory)
                        || string.Equals(t.Category, _clientOptions.VideoCategory, StringComparison.OrdinalIgnoreCase)));

                if (explicitTemplate != null)
                {
                    return explicitTemplate;
                }

                throw new CustomApplicationException($"Configured video template '{_clientOptions.VideoTemplateName}' was not found in ComfyBridge templates.");
            }

            var preferredCategory = string.IsNullOrWhiteSpace(_clientOptions.VideoCategory)
                ? "image2video"
                : _clientOptions.VideoCategory;

            var scored = templateList
                .Select(t => new
                {
                    Template = t,
                    Score = CalculateTemplateScore(t, preferredCategory)
                })
                .OrderByDescending(t => t.Score)
                .ToList();

            var best = scored.FirstOrDefault();
            if (best == null || best.Score <= 0)
            {
                throw new CustomApplicationException("No compatible ComfyBridge template was found for video generation.");
            }

            return best.Template;
        }

        private static int CalculateTemplateScore(TemplateDescriptor template, string preferredCategory)
        {
            var score = 0;

            if (!string.IsNullOrWhiteSpace(template.Category)
                && template.Category.Contains(preferredCategory, StringComparison.OrdinalIgnoreCase))
            {
                score += 120;
            }

            if (ContainsIgnoreCase(template.Name, "image2video") || ContainsIgnoreCase(template.Name, "i2v"))
            {
                score += 60;
            }

            if (template.HasAnyInput(ImageFields)) score += 30;
            if (template.HasAnyInput(PromptFields)) score += 15;
            if (template.HasAnyInput(WidthFields)) score += 5;
            if (template.HasAnyInput(HeightFields)) score += 5;
            if (template.HasAnyInput(FpsFields)) score += 5;

            return score;
        }

        private Dictionary<string, object?> BuildPayload(TemplateDescriptor template, VideoGenerationRequest request, string comfyFileName)
        {
            var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            AddMappedValue(template, payload, ImageFields, comfyFileName);
            AddMappedValue(template, payload, PromptFields, request.Prompt);
            AddMappedValue(template, payload, WidthFields, request.Width > 0 ? request.Width : null);
            AddMappedValue(template, payload, HeightFields, request.Height > 0 ? request.Height : null);
            AddMappedValue(template, payload, FpsFields, request.Fps);
            AddMappedValue(template, payload, FrameCountFields, TryReadExtraOption(request.ExtraOptions, FrameCountFields));
            AddMappedValue(template, payload, NegativePromptFields, TryReadExtraOption(request.ExtraOptions, NegativePromptFields));

            foreach (var configuredOption in ToDictionary(_clientOptions.ExtraOptions))
            {
                var key = template.ResolveInputKey(configuredOption.Key);
                if (key != null)
                {
                    payload[key] = configuredOption.Value;
                }
            }

            foreach (var extraOption in ToDictionary(request.ExtraOptions))
            {
                var key = template.ResolveInputKey(extraOption.Key);
                if (key != null)
                {
                    payload[key] = extraOption.Value;
                }
            }

            return payload;
        }

        private static object? TryReadExtraOption(object? extraOptions, IEnumerable<string> keys)
        {
            var extra = ToDictionary(extraOptions);
            foreach (var key in keys)
            {
                if (extra.TryGetValue(key, out var value))
                {
                    return value;
                }
            }

            return null;
        }

        private static void AddMappedValue(
            TemplateDescriptor template,
            IDictionary<string, object?> payload,
            IEnumerable<string> candidateFields,
            object? value)
        {
            if (value == null)
            {
                return;
            }

            if (value is string text && string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var resolvedKey = template.ResolveInputKey(candidateFields);
            if (resolvedKey != null)
            {
                payload[resolvedKey] = value;
            }
        }

        private static Dictionary<string, object?> ToDictionary(object? value)
        {
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            if (value == null)
            {
                return result;
            }

            var element = JsonSerializer.SerializeToElement(value);
            if (element.ValueKind != JsonValueKind.Object)
            {
                return result;
            }

            foreach (var property in element.EnumerateObject())
            {
                result[property.Name] = ConvertJsonElement(property.Value);
            }

            return result;
        }

        private static object? ConvertJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Undefined => null,
                JsonValueKind.Null => null,
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number when element.TryGetInt64(out var l) => l,
                JsonValueKind.Number when element.TryGetDecimal(out var d) => d,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => JsonSerializer.Deserialize<object>(element.GetRawText())
            };
        }

        private static IReadOnlyList<string?> ExtractAssetUrls(JsonElement root)
        {
            if (TryGetProperty(root, "assetUrls", out var urlsElement)
                && urlsElement.ValueKind == JsonValueKind.Array)
            {
                return urlsElement.EnumerateArray().Select(a => a.GetString()).ToList();
            }

            if (TryGetProperty(root, "assets", out var assetsElement)
                && assetsElement.ValueKind == JsonValueKind.Array)
            {
                var urls = new List<string?>();
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    if (asset.ValueKind == JsonValueKind.String)
                    {
                        urls.Add(asset.GetString());
                        continue;
                    }

                    if (asset.ValueKind == JsonValueKind.Object)
                    {
                        urls.Add(GetString(asset, ["url", "assetUrl", "downloadUrl"]));
                    }
                }

                return urls;
            }

            return [];
        }

        private static int SelectBestVideoAssetIndex(IReadOnlyList<string?> assetUrls)
        {
            for (var i = 0; i < assetUrls.Count; i++)
            {
                var url = assetUrls[i] ?? string.Empty;
                if (url.Contains(".mp4", StringComparison.OrdinalIgnoreCase)
                    || url.Contains(".webm", StringComparison.OrdinalIgnoreCase)
                    || url.Contains(".mov", StringComparison.OrdinalIgnoreCase)
                    || url.Contains(".mkv", StringComparison.OrdinalIgnoreCase)
                    || url.Contains(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return 0;
        }

        private static IEnumerable<TemplateDescriptor> ParseTemplates(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        yield return ParseTemplate(item);
                    }
                }

                yield break;
            }

            if (root.ValueKind == JsonValueKind.Object
                && TryGetProperty(root, "templates", out var templatesElement)
                && templatesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in templatesElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        yield return ParseTemplate(item);
                    }
                }
            }
        }

        private static TemplateDescriptor ParseTemplate(JsonElement templateElement)
        {
            var name = GetString(templateElement, ["name", "Name"]) ?? string.Empty;
            var category = GetString(templateElement, ["category", "Category"]) ?? string.Empty;
            var version = GetString(templateElement, ["version", "Version"]);

            var inputs = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            if (TryGetProperty(templateElement, "inputs", out var inputsElement)
                && inputsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var inputProperty in inputsElement.EnumerateObject())
                {
                    inputs[inputProperty.Name] = inputProperty.Value;
                }
            }

            return new TemplateDescriptor(name, category, version, inputs);
        }

        private static bool IsCompletedStatus(string status)
        {
            return status.Equals("completed", StringComparison.OrdinalIgnoreCase)
                || status.Equals("complete", StringComparison.OrdinalIgnoreCase)
                || status.Equals("succeeded", StringComparison.OrdinalIgnoreCase)
                || status.Equals("success", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFailedStatus(string status)
        {
            return status.Equals("failed", StringComparison.OrdinalIgnoreCase)
                || status.Equals("error", StringComparison.OrdinalIgnoreCase)
                || status.Equals("cancelled", StringComparison.OrdinalIgnoreCase)
                || status.Equals("canceled", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsIgnoreCase(string? source, string value)
        {
            return !string.IsNullOrWhiteSpace(source)
                && source.Contains(value, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        private static string? GetString(JsonElement element, IEnumerable<string> propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (TryGetProperty(element, propertyName, out var propertyValue))
                {
                    if (propertyValue.ValueKind == JsonValueKind.String)
                    {
                        return propertyValue.GetString();
                    }

                    return propertyValue.GetRawText().Trim('"');
                }
            }

            return null;
        }

        private async Task<JsonElement> ReadJsonOrThrow(
            HttpResponseMessage response,
            string endpoint,
            CancellationToken token)
        {
            var body = await response.Content.ReadAsStringAsync(token);

            if (!response.IsSuccessStatusCode)
            {
                throw new ComfyBridgeApiException(
                    $"ComfyBridge request failed: {(int)response.StatusCode} {response.ReasonPhrase} on '{endpoint}'.",
                    response.StatusCode,
                    body);
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return JsonDocument.Parse("{}").RootElement.Clone();
            }

            return JsonDocument.Parse(body).RootElement.Clone();
        }

        private static async Task EnsureSuccess(HttpResponseMessage response, string endpoint, CancellationToken token)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync(token);
            throw new ComfyBridgeApiException(
                $"ComfyBridge binary output request failed: {(int)response.StatusCode} {response.ReasonPhrase} on '{endpoint}'.",
                response.StatusCode,
                body);
        }

        private sealed class TemplateDescriptor
        {
            public TemplateDescriptor(
                string name,
                string category,
                string? version,
                Dictionary<string, JsonElement> inputs)
            {
                Name = name;
                Category = category;
                Version = version;
                Inputs = inputs;
            }

            public string Name { get; }

            public string Category { get; }

            public string? Version { get; }

            public Dictionary<string, JsonElement> Inputs { get; }

            public bool HasAnyInput(IEnumerable<string> inputNames)
            {
                return ResolveInputKey(inputNames) != null;
            }

            public string? ResolveInputKey(string inputName)
            {
                return Inputs.Keys.FirstOrDefault(k => string.Equals(k, inputName, StringComparison.OrdinalIgnoreCase));
            }

            public string? ResolveInputKey(IEnumerable<string> candidateNames)
            {
                foreach (var candidateName in candidateNames)
                {
                    var resolved = ResolveInputKey(candidateName);
                    if (resolved != null)
                    {
                        return resolved;
                    }
                }

                return null;
            }
        }
    }
}
