using Microsoft.Extensions.Options;
using NetXP.IAs.ImageGeneratorAI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    // Motion 2.0 (image-based) implementation
    // Reference: https://docs.leonardo.ai/docs/generate-with-motion-2-motion-2-fast-using-text-prompts
    public class LeonardoMotion2VideoGenerator : IVideoGeneratorAI
    {
        private readonly HttpClient _client;
        private readonly HttpClient _clientNoHeaders;



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
                elements = new[]
                {
                    new {
                        akUUID = "74bea0cc-9942-4d45-9977-28c25078bfd4",
                        weight = 1
                    }
                }
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
                   startJson["motionVideoGenerationJob"]["generationId"].Value<string>()
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

                pollObj = JObject.Parse(pollBody)
                         ?? throw new Exception("Null poll JSON.");

                var status = pollObj["generations_by_pk"]["status"].Value<string>();
                if (status == "COMPLETE") break;
                if (status == "FAILED") throw new Exception("Motion2 generation failed.");
            }

            // Extract video URL (field names may vary; attempt common possibilities)
            var videoUrl =
                pollObj["generations_by_pk"]["generated_images"][0]["motionMP4URL"].Value<string>()
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
    }
}