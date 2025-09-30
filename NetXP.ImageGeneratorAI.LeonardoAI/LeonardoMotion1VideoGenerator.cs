using Microsoft.Extensions.Options;
using NetXP.IAs.ImageGeneratorAI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    // Motion 1 (existing SVD) implementation refactored from old ImageGeneratorAILeonardoAI.GenerateVideoFromImage
    public class LeonardoMotion1VideoGenerator : IVideoGeneratorAI
    {
        private readonly HttpClient _client;
        private readonly HttpClient _clientNoHeaders;

        public LeonardoMotion1VideoGenerator(IHttpClientFactory factory, IOptions<ImageGeneratorAIOptions> options)
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
            if (request.Version != MotionVersion.Motion1)
                throw new InvalidOperationException("This generator handles only Motion1.");

            if (string.IsNullOrEmpty(request.SourceImagePath) || !File.Exists(request.SourceImagePath))
                throw new ArgumentException("SourceImagePath must exist for Motion1 (image-to-video).", nameof(request.SourceImagePath));

            // 1. Get presigned upload slot
            using var presigned = await _client.PostAsync("api/rest/v1/init-image",
                new StringContent(JsonConvert.SerializeObject(new { extension = "jpg" }), Encoding.UTF8, "application/json"), token);

            var presignedBody = await presigned.Content.ReadAsStringAsync(token);
            if (!presigned.IsSuccessStatusCode)
                throw new Exception($"Error getting presigned URL: {presigned.StatusCode} {presignedBody}");

            var presignedJson = JObject.Parse(presignedBody);
            var uploadNode = presignedJson["uploadInitImage"] ?? throw new Exception("uploadInitImage node missing.");
            var fields = JObject.Parse(uploadNode["fields"]!.ToString());
            var uploadUrl = uploadNode["url"]!.ToString();
            var imageId = uploadNode["id"]!.ToString();

            // 2. Upload image to storage
            var form = new MultipartFormDataContent();
            foreach (var f in fields)
                form.Add(new StringContent(f.Value!.ToString()), f.Key);

            var imgBytes = await File.ReadAllBytesAsync(request.SourceImagePath, token);
            var imgContent = new ByteArrayContent(imgBytes);
            imgContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            form.Add(imgContent, "file", Path.GetFileName(request.SourceImagePath));

            using var uploadResp = await _clientNoHeaders.PostAsync(uploadUrl, form, token);
            if (!uploadResp.IsSuccessStatusCode && uploadResp.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                var upBody = await uploadResp.Content.ReadAsStringAsync(token);
                throw new Exception($"Image upload failed: {uploadResp.StatusCode} {upBody}");
            }

            // 3. Start generation
            var generationPayload = new
            {
                imageId,
                isInitImage = true,
                motionStrength = request.MotionStrength ?? 5
            };
            using var startResp = await _client.PostAsync("api/rest/v1/generations-motion-svd",
                new StringContent(JsonConvert.SerializeObject(generationPayload), Encoding.UTF8, "application/json"), token);

            var startBody = await startResp.Content.ReadAsStringAsync(token);
            if (!startResp.IsSuccessStatusCode)
                throw new Exception($"Motion1 start failed: {startResp.StatusCode} {startBody}");

            var startJson = JObject.Parse(startBody);
            var generationId = startJson["motionSvdGenerationJob"]?["generationId"]?.ToString()
                               ?? throw new Exception("generationId missing from motionSvdGenerationJob.");

            // 4. Poll
            LeonardoAIVideoGeneratedRoot pollObj;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(1000, token);
                using var pollResp = await _client.GetAsync($"api/rest/v1/generations/{generationId}", token);
                var pollBody = await pollResp.Content.ReadAsStringAsync(token);
                if (!pollResp.IsSuccessStatusCode)
                    throw new Exception($"Polling failed: {pollResp.StatusCode} {pollBody}");
                pollObj = JsonConvert.DeserializeObject<LeonardoAIVideoGeneratedRoot>(pollBody)
                          ?? throw new Exception("Null polling response JSON.");
                var status = pollObj.GenerationsByPk?.Status;
                if (status == "COMPLETE") break;
                if (status == "FAILED") throw new Exception("Motion1 generation failed.");
            }

            // 5. Download
            var videoUrl = pollObj.GenerationsByPk!.GeneratedImages.FirstOrDefault()?.MotionMP4URL
                           ?? throw new Exception("Video URL not found.");
            byte[] bytes = await _clientNoHeaders.GetByteArrayAsync(videoUrl, token);

            return new VideoGenerated
            {
                Id = generationId,
                Url = videoUrl,
                Video = bytes
            };
        }
    }
}