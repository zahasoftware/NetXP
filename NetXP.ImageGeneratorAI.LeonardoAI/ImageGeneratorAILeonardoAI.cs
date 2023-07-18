using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    public class ImageGeneratorAILeonardoAI : IImageGeneratorAI
    {
        private HttpClient client;
        private IOptions<ImageGeneratorAIOptions> options;

        public ImageGeneratorAILeonardoAI(IOptions<ImageGeneratorAIOptions> options, IHttpClientFactory client)
        {
            this.client = client.CreateClient();
            this.options = options;

            this.client.BaseAddress = new Uri("https://cloud.leonardo.ai/");
            this.client.DefaultRequestHeaders.Add("accept", "application/json");
            this.client.DefaultRequestHeaders.Add("authorization", $"Bearer {this.options.Value.Token}");
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

        public async Task<ResultImagesGenerated> GetImages(ResultGenerate resultGenerate)
        {
            var httpResponseMessage = await client.GetAsync($"api/rest/v1/generations/{resultGenerate.Id}");

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"{await httpResponseMessage.Content.ReadAsStringAsync()}");
            }

            //var GenerateImageLeonardoAIRoot;
            var stringResult = await httpResponseMessage.Content.ReadAsStringAsync();
            GenerateImageLeonardoAIRoot result = JsonConvert.DeserializeObject<GenerateImageLeonardoAIRoot>(stringResult,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DateFormatString = "yyyy-MM-ddTHH:mm:ss.zzz"
                    });


            var resultImagesGenerated = new ResultImagesGenerated();

            if (result == null || result.generations_by_pk.status == "PENDING")
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
                        Image = await this.client.GetByteArrayAsync(r.url)
                    });
                }
            }

            return resultImagesGenerated;
        }

    }
}