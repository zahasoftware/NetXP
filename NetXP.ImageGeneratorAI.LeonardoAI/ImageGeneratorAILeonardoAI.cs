using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    public class ImageGeneratorAILeonardoAI : IImageGeneratorAI
    {
        private HttpClient client;
        private IHttpClientFactory clientFactory;
        private IOptions<ImageGeneratorAIOptions> options;
        private HttpClient clientForAWS;

        public ImageGeneratorAILeonardoAI(IOptions<ImageGeneratorAIOptions> options, IHttpClientFactory clientFactory)
        {
            client = clientFactory.CreateClient();
            this.clientFactory = clientFactory;
            this.options = options;

            client.BaseAddress = new Uri("https://cloud.leonardo.ai/");
            client.DefaultRequestHeaders.Add("accept", "application/json");
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {options.Value.Token}");

            clientForAWS = clientFactory.CreateClient();
            clientForAWS.DefaultRequestHeaders.Accept.Clear();
        }

        public async Task<ResultGenerate> Generate(OptionsImageGenerator oig)
        {
            dynamic mainContent = new
            {
                prompt = oig.Prompt,
                negative_prompt = oig.NegativePrompt,
                width = oig.Width,
                height = oig.Height,
                modelId = oig.ModelId,
                num_images = oig.NumImages
            };

            dynamic mergedContent = TypeMerger.TypeMerger.Merge(mainContent, oig.ExtraOptions ?? new { });

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            string contentOtptionsAsJsonString = JsonConvert.SerializeObject(mergedContent, settings);

            //Sending otions 
            var httpContent = new StringContent(contentOtptionsAsJsonString, Encoding.UTF8, "application/json");
            var httpResponseMessage = await client.PostAsync("api/rest/v1/generations", httpContent);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"{await httpResponseMessage.Content.ReadAsStringAsync()}");
            }

            var resultGenerate = await httpResponseMessage.Content.ReadFromJsonAsync<SdGenerationJobRoot>();
            return new ResultGenerate { Id = resultGenerate?.SdGenerationJob?.GenerationId };
        }

        public async Task<ResultImagesGenerated?> GetImages(ResultGenerate resultGenerate)
        {
            var httpResponseMessage = await client.GetAsync($"api/rest/v1/generations/{resultGenerate.Id}");

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"{await httpResponseMessage.Content.ReadAsStringAsync()}");
            }

            //var GenerateImageLeonardoAIRoot;
            var stringResult = await httpResponseMessage.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(stringResult))
            {
                throw new ArgumentNullException(nameof(stringResult));
            }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            GenerateImageLeonardoAIRoot result = JsonConvert.DeserializeObject<GenerateImageLeonardoAIRoot>(stringResult,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DateFormatString = "yyyy-MM-ddTHH:mm:ss.zzz"
                    });
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.


            var resultImagesGenerated = new ResultImagesGenerated();

            if (result == null || result.generations_by_pk == null || result.generations_by_pk.status == "PENDING")
            {
                return null;
            }
            else
            {
                foreach (var r in result.generations_by_pk.generated_images)
                {
                    resultImagesGenerated.Images.Add(new ImageGenerate
                    {
                        Id = r.id,
                        Url = r.url,
                        Image = await client.GetByteArrayAsync(r.url)
                    });
                }
            }

            return resultImagesGenerated;
        }

        public async Task Remove(ResultGenerate resultGenerate)
        {
            var httpResponseMessage = await client.DeleteAsync($"api/rest/v1/generations/{resultGenerate.Id}");

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"{await httpResponseMessage.Content.ReadAsStringAsync()}");
            }
        }

        public async Task<VideoGenerated> GenerateVideoFromImage(ParameterVideoGenerator @params)
        {
            var response = await client.PostAsync("api/rest/v1/init-image", new StringContent(JsonConvert.SerializeObject(new { extension = "jpg" })));
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseContent);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error getting the presigned URL for uploading an image: " +
                    $"StatusCode: {response.StatusCode},  {message}");
            }

            //Calling to AWS to Upload the image
            var fields = JObject.Parse(jsonResponse["uploadInitImage"]["fields"].ToString());
            var url = jsonResponse["uploadInitImage"]["url"].ToString();
            var imageId = jsonResponse["uploadInitImage"]["id"].ToString(); // For getting the image later

            //Validating the image
            if (!System.IO.File.Exists(@params.ImageUrlOrPath))
            {
                throw new Exception($"The image file {@params.ImageUrlOrPath} does not exist.");
            }

            var imageFilePath = @params.ImageUrlOrPath;
            var imageContent = new ByteArrayContent(System.IO.File.ReadAllBytes(imageFilePath));
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var formContent = new MultipartFormDataContent();
            foreach (var field in fields)
            {
                formContent.Add(new StringContent(field.Value!.ToString()), field.Key);
            }
            formContent.Add(imageContent, "file", Path.GetFileName(imageFilePath));

            response = await clientForAWS.PostAsync(url, formContent);

            //Validating the response   
            if (response.StatusCode != System.Net.HttpStatusCode.OK 
             && response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error uploading the image via presigned URL: " +
                    $"StatusCode: {response.StatusCode},  {responseContent}");
            }

            //Generate video with an init image
            var jsonPayload = JsonConvert.SerializeObject(new
            {
                imageId,
                isInitImage = true,
                motionStrength = 5
            });
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            response = await client.PostAsync("api/rest/v1/generations-motion-svd", httpContent);

            //validating the response   
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error generating video with an init image: " +
                    $"StatusCode: {response.StatusCode},  {responseContent}");
            }

            jsonResponse = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
            var generation_id = jsonResponse["motionSvdGenerationJob"]["generationId"].ToString();

            LeonardoAIVideoGeneratedRoot json;
            while (true)
            {
                await Task.Delay(5);
                response = await client.GetAsync($"api/rest/v1/generations/{generation_id}");

                //validating the response   
                responseContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Error getting the status of the generation: " +
                        $"StatusCode: {response.StatusCode},  {responseContent}");
                }

                //validate responseContent if is null
                if (string.IsNullOrEmpty(responseContent))
                {
                    throw new ArgumentNullException(nameof(responseContent));
                }

                json = JsonConvert.DeserializeObject<LeonardoAIVideoGeneratedRoot>(responseContent)!;

                if (json == null)
                {
                    throw new ArgumentNullException(nameof(json));
                }

                if (json.GenerationsByPk.Status == "COMPLETE")
                {
                    break;
                }
            }

            //Downloading the video using json.Url
            var video = await client.GetByteArrayAsync(json.GenerationsByPk.GeneratedImages.FirstOrDefault().MotionMP4URL);

            return new VideoGenerated
            {
                Id = generation_id,
                Url = url,
                Video = video
            };
        }



    }

}