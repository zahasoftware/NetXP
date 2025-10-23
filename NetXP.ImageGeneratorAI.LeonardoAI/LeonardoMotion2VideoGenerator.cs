using Microsoft.Extensions.Options;
using NetXP.IAs.ImageGeneratorAI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Linq; // Added for LINQ operations

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    // Motion 2.0 (image-based) implementation
    // Reference: https://docs.leonardo.ai/docs/generate-with-motion-2-motion-2-fast-using-text-prompts
    public class LeonardoMotion2VideoGenerator : IVideoGeneratorAI
    {
        private readonly HttpClient _client;
        private readonly HttpClient _clientNoHeaders;

        // Mapping of cinematic / motion effect names to Leonardo Motion2 akUUID values.
        private static readonly Dictionary<string, string> Motion2EffectAkUuidMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Bullet Time", "fbed015e-594e-4f78-b4be-3b07142aaa1e" },
            { "Crane Down", "5a1d2a6a-7709-4097-9158-1b7ae6c9e647" },
            { "Crane Over Head", "1054d533-168c-4821-bd3d-a56182afa4f3" },
            { "Crane Up", "c765bd57-cdc5-4317-a600-69a8bd6c4ce6" },
            { "Crash Zoom In", "b0191ad1-a723-439c-a4bc-a3f5d5884db3" },
            { "Crash Zoom Out", "1975ac74-92ca-46b3-81b3-6f191a9ae438" },
            { "Disintegration", "a51e2e8d-ba5e-44f2-9e00-3d86fd93c9bc" },
            { "Dolly In", "ece8c6a9-3deb-430e-8c93-4d5061b6adbf" },
            { "Dolly Left", "f507880a-3fa8-4c3a-96bb-3ce3b70ac53b" },
            { "Dolly Out", "772cb36a-7d18-4250-b4aa-0c3f1a8431a0" },
            { "Dolly Right", "587a0109-30be-4781-a18e-e353b580fd10" },
            { "Explosion", "65da803d-c015-495a-8d5c-e969a79c9894" },
            { "Eyes In", "148b50d0-2040-4524-a36f-6e330f9e362e" },
            { "Flood", "a12c150e-95e9-469b-ba9b-6d5323ac5a09" },
            { "Handheld", "75722d13-108f-4cea-9471-cb7e5fc049fe" },
            { "Lens Crack", "193da194-2632-4f6a-a1df-d03ca9ae0ea9" },
            { "Medium Zoom In", "f46d8e7f-e0ca-4f6a-90ab-141d731f47ae" },
            { "Orbit Left", "74bea0cc-9942-4d45-9977-28c25078bfd4" },
            { "Orbit Right", "aec24e36-a2e8-4fae-920c-127d276bbe4b" },
            { "Robo Arm", "8df55fe2-5c6f-4dbf-8ade-eb997807ca0d" },
            { "Super Dolly In", "a3992d78-34fc-44c6-b157-e2755d905197" },
            { "Super Dolly Out", "906b93f2-beb3-42be-9283-92236cc90ed6" },
            { "Tilt Down", "a1923b1b-854a-46a1-9e26-07c435098b87" },
            { "Tilt Up", "6ad6de1f-bd15-4d0b-ae0e-81d1a4c6c085" }
        };

        public LeonardoMotion2VideoGenerator(IHttpClientFactory factory, IOptions<ImageGeneratorAIOptions> options)
        {
            _client = factory.CreateClient();
            _client.BaseAddress = new Uri("https://cloud.leonardo.ai/");
            _client.DefaultRequestHeaders.Add("accept", "application/json");
            _client.DefaultRequestHeaders.Add("authorization", $"Bearer {options.Value.Token}");

            _clientNoHeaders = factory.CreateClient();
            _clientNoHeaders.DefaultRequestHeaders.Accept.Clear();
        }

        public async Task<VideoGenerated> GenerateVideoAsync(VideoGenerationRequest request, CancellationToken token = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Version != MotionVersion.Motion2)
                throw new InvalidOperationException("This generator currently supports only Motion2 prompt-based generation.");
            if (string.IsNullOrWhiteSpace(request.Prompt))
                throw new ArgumentException("Prompt is required for text-to-video Motion2 generation.", nameof(request.Prompt));

            token.ThrowIfCancellationRequested();

            // Auto infer a resolution label if not explicitly provided via ExtraOptions.
            // Leonardo requires resolution enum like: RESOLUTION_480, RESOLUTION_720, etc.
            string resolution = InferResolutionLabel(request.Height, request.Width) ?? "RESOLUTION_480";

            // Get random element or use 'effects' from ExtraOptions if provided.
            IEnumerable<object> elements = Enumerable.Empty<object>();

            if (request.ExtraOptions is not null)
            {
                try
                {
                    var extra = JObject.FromObject(request.ExtraOptions);
                    var effectsToken = extra["effects"];
                    if (effectsToken is JArray arr && arr.Count > 0)
                    {
                        var effectNames = arr.Values<string>()
                                             .Where(s => !string.IsNullOrWhiteSpace(s));
                        var mapped = effectNames
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Select(n =>
                            {
                                if (Motion2EffectAkUuidMap.TryGetValue(n, out var id))
                                    return new { akUUID = id, weight = 1 };
                                return null;
                            })
                            .Where(o => o != null)!;
                        if (mapped.Any())
                            elements = mapped;
                    }
                }
                catch
                {
                    // Ignore parsing issues and fallback to random
                }
            }

            if (!elements.Any())
            {
                // Fallback: pick a single random effect from the map
                var randomIndex = Random.Shared.Next(Motion2EffectAkUuidMap.Count);
                var randomEntry = Motion2EffectAkUuidMap.ElementAt(randomIndex);
                elements = new[] { new { akUUID = randomEntry.Value, weight = 1 } };
            }

            // Base payload following cURL example; request.ExtraOptions can override/extend.
            var payload = new
            {
                height = request.Height > 0 ? request.Height : 480,
                width = request.Width > 0 ? request.Width : 832,
                resolution,
                prompt = request.Prompt,
                frameInterpolation = true,
                isPublic = false,
                promptEnhance = true,
                // If Variant provided (e.g., "MOTION_2" | "MOTION_2_FAST") include it; docs sometimes call it "model".
                model = string.IsNullOrWhiteSpace(request.Variant) ? "MOTION_2" : request.Variant,
                // Optional FPS if supplied
                fps = request.Fps,
                // Example elements array (caller can override via ExtraOptions)
                elements = elements
            };

            // Merge with ExtraOptions (if provided) so caller can inject advanced flags.
            dynamic merged = TypeMerger.TypeMerger.Merge(payload, request.ExtraOptions ?? new { });

            const string endpoint = "api/rest/v1/generations-text-to-video";

            using var startResp = await _client.PostAsync(
                endpoint,
                new StringContent(JsonConvert.SerializeObject(merged, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8, "application/json"),
                token);

            var startBody = await startResp.Content.ReadAsStringAsync(token);
            if (!startResp.IsSuccessStatusCode)
                throw new Exception($"Motion2 text-to-video start failed: {startResp.StatusCode} {startBody}");

            var startJson = JObject.Parse(startBody);

            // Attempt to extract generationId from various possible nodes (API responses sometimes differ)
            var generationId =
                startJson["motionVideoGenerationJob"]?["generationId"]?.Value<string>()
                ?? throw new Exception("generationId not found in start response.");

            // Poll for completion
            JObject pollObj;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(1500, token);

                using var pollResp = await _client.GetAsync($"api/rest/v1/generations/{generationId}", token);
                var pollBody = await pollResp.Content.ReadAsStringAsync(token);
                if (!pollResp.IsSuccessStatusCode)
                    throw new Exception($"Polling failed: {pollResp.StatusCode} {pollBody}");

                pollObj = JObject.Parse(pollBody) ?? throw new Exception("Null poll JSON.");
                var status = pollObj["generations_by_pk"]?["status"]?.Value<string>();
                if (status == "COMPLETE") break;
                if (status == "FAILED") throw new Exception("Motion2 generation failed.");
            }

            // Extract video URL (field names may vary; attempt common possibilities)
            var videoUrl =
                pollObj["generations_by_pk"]?["generated_images"]?[0]?["motionMP4URL"]?.Value<string>()
                ?? throw new Exception("Motion2 video URL not found in completed generation.");

            var videoBytes = await _clientNoHeaders.GetByteArrayAsync(videoUrl, token);

            return new VideoGenerated
            {
                Id = generationId,
                Url = videoUrl,
                Video = videoBytes
            };
        }

        public async Task<VideoGenerated> GenerateVideoAsyncNotWorkingWithInitImageSoFar(VideoGenerationRequest request, CancellationToken token = default)
        {
            if (request.Version != MotionVersion.Motion2)
                throw new InvalidOperationException("This generator handles only Motion2.");

            // Motion 2 image-based requires an image already generated or uploaded similarly to Motion1:
            string imageId;
            if (!string.IsNullOrEmpty(request.SourceImageId))
            {
                imageId = request.SourceImageId;
            }
            else if (!string.IsNullOrEmpty(request.SourceImagePath) && File.Exists(request.SourceImagePath))
            {
                // Re-use Motion1 upload logic (init-image)
                using var presigned = await _client.PostAsync("api/rest/v1/init-image",
                    new StringContent(JsonConvert.SerializeObject(new { extension = "jpg" }), Encoding.UTF8, "application/json"), token);

                var body = await presigned.Content.ReadAsStringAsync(token);
                if (!presigned.IsSuccessStatusCode)
                    throw new Exception($"Presign failed: {presigned.StatusCode} {body}");

                var json = JObject.Parse(body);
                var uploadNode = json["uploadInitImage"] ?? throw new Exception("uploadInitImage missing.");
                var fields = JObject.Parse(uploadNode["fields"]!.ToString());
                var uploadUrl = uploadNode["url"]!.ToString();
                imageId = uploadNode["id"]!.ToString();


                var form = new MultipartFormDataContent();
                foreach (var f in fields)
                    form.Add(new StringContent(f.Value!.ToString()), f.Key);


                var bytes = await File.ReadAllBytesAsync(request.SourceImagePath, token);
                var content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                form.Add(content, "file", Path.GetFileName(request.SourceImagePath));
                using var uploadResp = await _clientNoHeaders.PostAsync(uploadUrl, form, token);
                if (!uploadResp.IsSuccessStatusCode && uploadResp.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    var upBody = await uploadResp.Content.ReadAsStringAsync(token);
                    throw new Exception($"Image upload failed: {uploadResp.StatusCode} {upBody}");
                }

            }
            else
            {
                throw new ArgumentException("Provide either SourceImageId or a valid SourceImagePath for Motion2 image-based generation.");
            }

            // Start Motion2 image-based generation
            // Placeholder endpoint; adjust if docs specify a different path.
            const string motion2Endpoint = "api/rest/v1/generations-image-to-video"; // verify against docs
            var payload = new
            {
                imageId,
                isPublic = false,
                prompt = request.Prompt,
                model = request.Variant,          // MOTION_2 or MOTION_2_FAST
                promptEnhance = false,
                imageType = "GENERATED"
            };
            dynamic merged = TypeMerger.TypeMerger.Merge(payload, request.ExtraOptions ?? new { });

            using var startResp = await _client.PostAsync(motion2Endpoint,
                new StringContent(JsonConvert.SerializeObject(merged, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8, "application/json"), token);

            var startBody = await startResp.Content.ReadAsStringAsync(token);
            if (!startResp.IsSuccessStatusCode)
                throw new Exception($"Motion2 start failed: {startResp.StatusCode} {startBody}");

            var startJson = JObject.Parse(startBody);
            var generationId = startJson.SelectToken("motionGenerationJob.generationId")?.ToString()
                               ?? startJson.SelectToken("motion2GenerationJob.generationId")?.ToString()
                               ?? startJson.SelectToken("generationId")?.ToString()
                               ?? throw new Exception("generationId not found in Motion2 start response.");

            // Poll
            LeonardoAIVideoGeneratedRoot pollObj;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(1500, token);
                using var pollResp = await _client.GetAsync($"api/rest/v1/generations/{generationId}", token);
                var pollBody = await pollResp.Content.ReadAsStringAsync(token);
                if (!pollResp.IsSuccessStatusCode)
                    throw new Exception($"Polling failed: {pollResp.StatusCode} {pollBody}");
                pollObj = JsonConvert.DeserializeObject<LeonardoAIVideoGeneratedRoot>(pollBody)
                          ?? throw new Exception("Null Motion2 poll JSON.");
                var status = pollObj.GenerationsByPk?.Status;
                if (status == "COMPLETE") break;
                if (status == "FAILED") throw new Exception("Motion2 generation failed.");
            }

            var videoUrl = pollObj.GenerationsByPk!.GeneratedImages.FirstOrDefault()?.MotionMP4URL
                           ?? pollObj.GenerationsByPk.GeneratedImages.FirstOrDefault()?.Url
                           ?? throw new Exception("Motion2 video URL not found.");

            var videoBytes = await _clientNoHeaders.GetByteArrayAsync(videoUrl, token);

            return new VideoGenerated
            {
                Id = generationId,
                Url = videoUrl,
                Video = videoBytes
            };
        }

        private static string? InferResolutionLabel(int height, int width)
        {
            if (height == 480 || width == 480) return "RESOLUTION_480";
            if (height == 720 || width == 720) return "RESOLUTION_720";
            // Fallback heuristics
            if (height <= 480 || width <= 480) return "RESOLUTION_480";
            if (height <= 720 || width <= 720) return "RESOLUTION_720";

            return null;
        }

        // Helper: build 'elements' array objects accepted by Leonardo API from effect names.
        // Each element => { akUUID = "<guid>", weight = 1 }
        private static IEnumerable<object> BuildElementsFromNames(IEnumerable<string> effectNames, int weight = 1)
        {
            foreach (var name in effectNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (Motion2EffectAkUuidMap.TryGetValue(name, out var id))
                {
                    yield return new { akUUID = id, weight };
                }
            }
        }
    }
}