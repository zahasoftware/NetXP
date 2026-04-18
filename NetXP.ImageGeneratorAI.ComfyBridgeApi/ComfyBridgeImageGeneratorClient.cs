using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetXP.Exceptions;
using NetXP.IAs.ImageGeneratorAI;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace NetXP.ImageGeneratorAI.ComfyBridgeApi
{
    public class ComfyBridgeImageGeneratorClient : IImageGeneratorAI
    {
        private static readonly string[] PromptFields = ["prompt", "text"];
        private static readonly string[] NegativePromptFields = ["negativePrompt", "negative_prompt"];
        private static readonly string[] WidthFields = ["width", "imageWidth", "image_width"];
        private static readonly string[] HeightFields = ["height", "imageHeight", "image_height"];
        private static readonly string[] NumImagesFields = ["numImages", "num_images", "batch_size", "batchSize"];
        private static readonly string[] ModelFields = ["checkpointName", "checkpoint_name", "model", "modelId", "model_id", "checkpoint"];

        private readonly HttpClient _httpClient;
        private readonly ComfyBridgeClientOptions _clientOptions;
        private readonly ILogger<ComfyBridgeImageGeneratorClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ComfyBridgeImageGeneratorClient(
            HttpClient httpClient,
            IOptions<ComfyBridgeClientOptions> clientOptions,
            ILogger<ComfyBridgeImageGeneratorClient> logger)
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

        public Task<ResultGenerate> Generate(OptionsImageGenerator options)
        {
            return Generate(options, CancellationToken.None);
        }

        public async Task<ResultGenerate> Generate(OptionsImageGenerator options, CancellationToken cancellationToken)
        {
            ValidateGenerateOptions(options);

            var templates = await GetTemplatesInternal(cancellationToken);
            var selectedTemplate = SelectTemplate(templates, options);
            var payload = BuildPayload(selectedTemplate, options);

            var endpoint = $"/api/v1/{selectedTemplate.Category}/{selectedTemplate.Name}";

            _logger.LogInformation(
                "Submitting ComfyBridge generation request to {Endpoint} using template {Category}/{Name} with {PayloadCount} fields.",
                endpoint,
                selectedTemplate.Category,
                selectedTemplate.Name,
                payload.Count);

            using var response = await _httpClient.PostAsJsonAsync(endpoint, payload, _jsonOptions, cancellationToken);
            var root = await ReadJsonOrThrow(response, endpoint, cancellationToken);

            var jobId = GetString(root, ["jobId", "JobId", "id", "Id"]);
            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new CustomApplicationException("ComfyBridge did not return a valid JobId.");
            }

            return new ResultGenerate { Id = jobId };
        }

        public Task<ResultImagesGenerated> GetImages(ResultGenerate resultGenerate)
        {
            return GetImages(resultGenerate, CancellationToken.None);
        }

        public async Task<ResultImagesGenerated> GetImages(ResultGenerate resultGenerate, CancellationToken cancellationToken)
        {
            if (resultGenerate == null || string.IsNullOrWhiteSpace(resultGenerate.Id))
            {
                throw new ArgumentException("ResultGenerate.Id is required.", nameof(resultGenerate));
            }

            var jobId = resultGenerate.Id;
            var pollDelay = _clientOptions.GetPollInterval();
            var maxPollDelay = TimeSpan.FromSeconds(5);
            var timeout = _clientOptions.GetTimeout();
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "Polling ComfyBridge job {JobId} every {PollMs}ms for up to {TimeoutSeconds}s.",
                jobId,
                pollDelay.TotalMilliseconds,
                timeout.TotalSeconds);

            while (stopwatch.Elapsed < timeout)
            {
                var endpoint = $"/api/v1/jobs/{jobId}";
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                var root = await ReadJsonOrThrow(response, endpoint, cancellationToken);

                var status = GetString(root, ["status", "Status", "jobStatus"]);
                if (string.IsNullOrWhiteSpace(status))
                {
                    _logger.LogWarning("ComfyBridge job {JobId} returned empty status. Retrying.", jobId);
                }
                else if (IsCompletedStatus(status))
                {
                    var urls = ExtractAssetUrls(root);
                    return await DownloadImages(jobId, urls, cancellationToken);
                }
                else if (IsFailedStatus(status))
                {
                    var errorMessage = GetString(root, ["errorMessage", "ErrorMessage", "error", "message"]);
                    throw new ComfyBridgeJobFailedException(jobId, errorMessage);
                }

                await Task.Delay(pollDelay, cancellationToken);
                var nextDelayMs = Math.Min(pollDelay.TotalMilliseconds * 1.5d, maxPollDelay.TotalMilliseconds);
                pollDelay = TimeSpan.FromMilliseconds(nextDelayMs);
            }

            throw new TimeoutException($"ComfyBridge job '{jobId}' did not finish before timeout ({timeout.TotalSeconds}s).");
        }

        public Task Remove(ResultGenerate resultGenerate)
        {
            return Remove(resultGenerate, CancellationToken.None);
        }

        public Task Remove(ResultGenerate resultGenerate, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (resultGenerate == null || string.IsNullOrWhiteSpace(resultGenerate.Id))
            {
                throw new ArgumentException("ResultGenerate.Id is required.", nameof(resultGenerate));
            }

            _logger.LogWarning(
                "Remove called for ComfyBridge job {JobId}, but API does not expose delete endpoints. No action taken.",
                resultGenerate.Id);

            return Task.CompletedTask;
        }

        public Task<List<ImageModel>> GetModels()
        {
            return GetModels(CancellationToken.None);
        }

        public async Task<List<ImageModel>> GetModels(CancellationToken cancellationToken)
        {
            var templates = await GetTemplatesInternal(cancellationToken);
            return templates
                .Select(template => new ImageModel
                {
                    Id = BuildTemplateId(template),
                    Name = string.IsNullOrWhiteSpace(template.Version)
                        ? template.Name
                        : $"{template.Name} ({template.Version})",
                    Description = BuildTemplateDescription(template)
                })
                .ToList();
        }

        private async Task<List<TemplateDescriptor>> GetTemplatesInternal(CancellationToken cancellationToken)
        {
            const string endpoint = "/api/v1/templates";

            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            var root = await ReadJsonOrThrow(response, endpoint, cancellationToken);

            var templates = ParseTemplates(root).ToList();
            if (templates.Count == 0)
            {
                throw new CustomApplicationException("ComfyBridge did not return templates.");
            }

            return templates;
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

        private TemplateDescriptor SelectTemplate(IEnumerable<TemplateDescriptor> templates, OptionsImageGenerator options)
        {
            var templateList = templates.ToList();

            if (!string.IsNullOrWhiteSpace(_clientOptions.TemplateName))
            {
                var explicitTemplate = templateList.FirstOrDefault(t =>
                    string.Equals(t.Name, _clientOptions.TemplateName, StringComparison.OrdinalIgnoreCase)
                    && (string.IsNullOrWhiteSpace(_clientOptions.Category)
                        || string.Equals(t.Category, _clientOptions.Category, StringComparison.OrdinalIgnoreCase)));

                if (explicitTemplate != null)
                {
                    return explicitTemplate;
                }

                throw new CustomApplicationException(
                    $"Configured template '{_clientOptions.TemplateName}' was not found in ComfyBridge templates.");
            }

            var scoredTemplates = templateList
                .Select(t => new
                {
                    Template = t,
                    Score = CalculateTemplateScore(t, options)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            var bestTemplate = scoredTemplates.FirstOrDefault();
            if (bestTemplate == null || bestTemplate.Score <= 0)
            {
                throw new CustomApplicationException("No compatible ComfyBridge template was found for image generation.");
            }

            return bestTemplate.Template;
        }

        private static int CalculateTemplateScore(TemplateDescriptor template, OptionsImageGenerator options)
        {
            var score = 0;

            if (ContainsIgnoreCase(template.Name, "text2image") || ContainsIgnoreCase(template.Name, "txt2img"))
            {
                score += 100;
            }

            if (ContainsIgnoreCase(template.Category, "image"))
            {
                score += 10;
            }

            if (template.HasAnyInput(PromptFields)) score += 20;
            if (template.HasAnyInput(NegativePromptFields) && !string.IsNullOrWhiteSpace(options.NegativePrompt)) score += 8;
            if (template.HasAnyInput(WidthFields)) score += 10;
            if (template.HasAnyInput(HeightFields)) score += 10;
            if (template.HasAnyInput(NumImagesFields)) score += 8;
            if (template.HasAnyInput(ModelFields) && !string.IsNullOrWhiteSpace(options.ModelId)) score += 6;

            return score;
        }

        private static Dictionary<string, object?> BuildPayload(TemplateDescriptor template, OptionsImageGenerator options)
        {
            var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            AddMappedValue(template, payload, PromptFields, options.Prompt);
            AddMappedValue(template, payload, NegativePromptFields, options.NegativePrompt);
            AddMappedValue(template, payload, WidthFields, options.Width);
            AddMappedValue(template, payload, HeightFields, options.Height);
            AddMappedValue(template, payload, NumImagesFields, options.NumImages);
            AddMappedValue(template, payload, ModelFields, options.ModelId);

            foreach (var extraOption in ToDictionary(options.ExtraOptions))
            {
                var key = template.ResolveInputKey(extraOption.Key);
                if (key != null)
                {
                    payload[key] = extraOption.Value;
                }
            }

            return payload;
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

        private async Task<ResultImagesGenerated> DownloadImages(
            string jobId,
            IReadOnlyList<string?> assetUrls,
            CancellationToken cancellationToken)
        {
            var result = new ResultImagesGenerated();

            for (var index = 0; index < assetUrls.Count; index++)
            {
                var endpoint = $"/api/v1/jobs/{jobId}/outputs/{index}";
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                await EnsureSuccess(response, endpoint, cancellationToken);

                var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                result.Images.Add(new ImageGenerate
                {
                    Id = $"{jobId}:{index}",
                    Url = assetUrls[index] ?? string.Empty,
                    Image = imageBytes
                });
            }

            return result;
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

        private static void ValidateGenerateOptions(OptionsImageGenerator options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.Prompt))
            {
                throw new ArgumentException("Prompt is required.", nameof(options));
            }

            if (options.Width <= 0)
            {
                throw new ArgumentException("Width must be greater than zero.", nameof(options));
            }

            if (options.Height <= 0)
            {
                throw new ArgumentException("Height must be greater than zero.", nameof(options));
            }

            if (options.NumImages <= 0)
            {
                throw new ArgumentException("NumImages must be greater than zero.", nameof(options));
            }
        }

        private static string BuildTemplateId(TemplateDescriptor template)
        {
            return $"{template.Category}:{template.Name}:{template.Version ?? "latest"}";
        }

        private static string BuildTemplateDescription(TemplateDescriptor template)
        {
            var inputs = string.Join(", ", template.Inputs.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase));
            var supportsModel = template.HasAnyInput(ModelFields) ? "supports model field" : "no model field";
            return $"Category: {template.Category}. Inputs: {inputs}. {supportsModel}.";
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
            CancellationToken cancellationToken)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

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

        private async Task EnsureSuccess(
            HttpResponseMessage response,
            string endpoint,
            CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
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