using Microsoft.Extensions.Options;
using NetXP.ImageGeneratorAI;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Web;
using System.IO;

namespace NetXP.TTS.OpenTTS
{
    public class TTSOpenTTS : ITTS
    {
        private readonly HttpClient client;
        private readonly IOptions<TTSOptions> options;

        public TTSOpenTTS(IOptions<TTSOptions> options, IHttpClientFactory client)
        {
            this.client = client.CreateClient();
            this.options = options;

            this.client.BaseAddress = new Uri(options.Value.URL);
            this.client.DefaultRequestHeaders.Add("accept", "application/json");
        }

        public async Task<TTSAudio> Convert(TTSConvertOption ttsConvertOption)
        {
            //Query parameters
            var parameters = new
            {
                voice = $"{ttsConvertOption.Voice.TtsName}:{ttsConvertOption.Voice.Id}",
                text = ttsConvertOption.Text,
                vocoder = "high",
                denoiserStrength = 0.03,
                cache = false
            };

            // Generate the URL
            var baseUrl = $"{client.BaseAddress}/api/tts";
            var urlBuilder = new UriBuilder(baseUrl);
            var query = HttpUtility.ParseQueryString(urlBuilder.Query);

            // Add each parameter to the query string
            foreach (var property in parameters.GetType().GetProperties())
            {
                var value = property.GetValue(parameters);
                if (value != null)
                {
                    query[property.Name] = value.ToString();
                }
            }

            urlBuilder.Query = query.ToString();
            var finalUrl = urlBuilder.ToString();

            HttpResponseMessage response = await client.GetAsync(finalUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"{await response.Content.ReadAsStringAsync()}");
            }
            
            var audioToReturn = new TTSAudio()
            {
                Format = response?.Content?.Headers?.ContentType?.MediaType,
                File = new MemoryStream()
            };
            var streamResponse = await response.Content.ReadAsStreamAsync();
            await streamResponse.CopyToAsync(audioToReturn.File);

            return audioToReturn;
        }

        public async Task<List<TTSVoice>> GetTTSVoices(string language)
        {
            //Sending otions 
            var httpResponseMessage = await client.GetFromJsonAsync<Dictionary<string, OpenTtsModelResponse>>("/api/voices?language={language}");

            var voices = new List<TTSVoice> { };

            if (httpResponseMessage != null)
                foreach (var key in httpResponseMessage?.Keys)
                {
                    voices.Add(new TTSVoice
                    {
                        Id = httpResponseMessage[key].Id,
                        Gender = httpResponseMessage[key].Gender,
                        TtsName = httpResponseMessage[key].tts_name,
                        Language = httpResponseMessage[key].Language,
                        Name = httpResponseMessage[key].Name,
                    });
                }

            return voices;
        }
    }
}