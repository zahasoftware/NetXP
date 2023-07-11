using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Json;

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
                oig.Prompt,
                oig.NegativePrompt,
                oig.Width,
                oig.Height,
                oig.ModelId,
                oig.NumImages
            };

            dynamic mergedContent = TypeMerger.TypeMerger.Merge(mainContent, oig.ExtraOptions);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            string contentOtptionsAsJsonString = JsonConvert.SerializeObject(mergedContent, settings);

            //Sending otions 
            var httpResponseMessage = await client.PostAsJsonAsync("api/rest/v1/generations", contentOtptionsAsJsonString);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"{await httpResponseMessage.Content.ReadAsStringAsync()}");
            }

            var resultGenerate = await httpResponseMessage.Content.ReadFromJsonAsync<SdGenerationJobRoot>();
            return new ResultGenerate { Id = resultGenerate?.SdGenerationJob?.GenerationId };
        }

        public async Task<ResultImagesGenerated> GetImages(ResultGenerate resultGenerate)
        {
            string content = JsonConvert.SerializeObject(resultGenerate);

            var httpResponseMessage = await client.PostAsJsonAsync("api/rest/v1/generations/id", content);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"{await httpResponseMessage.Content.ReadAsStringAsync()}");
            }

            var result = await httpResponseMessage.Content.ReadFromJsonAsync<GenerateImageLeonardoAIRoot>();

            var resultImagesGenerated = new ResultImagesGenerated();

            if (result != null)
            {
                foreach (var r in result.generations_by_pk.generated_images)
                {
                    resultImagesGenerated.Images.Add(new ImageGenerate
                    {
                        Id = r.id,
                        Url = r.url
                    });
                }
            }

            return resultImagesGenerated;
        }

          }
}