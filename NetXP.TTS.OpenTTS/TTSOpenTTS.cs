using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Web;
using System.IO;
using NetXP.Tts;

namespace NetXP.TTSs.OpenTTS
{
    public class TTSOpenTTS : ITts
    {
        private readonly HttpClient client;
        private readonly IOptions<TtsOptions> options;

        public TTSOpenTTS(IOptions<TtsOptions> options, IHttpClientFactory client)
        {
            this.client = client.CreateClient();
            this.options = options;

            this.client.BaseAddress = new Uri(options.Value.URL);
            this.client.DefaultRequestHeaders.Add("accept", "application/json");
        }

        public async Task<TtsAudio> Convert(TtsConvertOption ttsConvertOption)
        {
            if (ttsConvertOption.Voice == null)
            {
                throw new NetXP.Exceptions.CustomApplicationException("Voice cannot be null in TtsConvertOption");
            }

            //Query parameters
            var parameters = new
            {
                voice = $"{ttsConvertOption.Voice.ModelId}:{ttsConvertOption.Voice.Id}",
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

            var audioToReturn = new TtsAudio()
            {
                Format = response?.Content?.Headers?.ContentType?.MediaType,
                File = new MemoryStream()
            };
            var streamResponse = await response.Content.ReadAsStreamAsync();
            await streamResponse.CopyToAsync(audioToReturn.File);

            return audioToReturn;
        }

        public async Task<TtsAudio> Convert(TtsConvertOption ttsConvertOption, CancellationToken token)
        {

            if (ttsConvertOption.Voice == null)
            {
                throw new NetXP.Exceptions.CustomApplicationException("Voice cannot be null in TtsConvertOption");
            }

            //Query parameters
            var parameters = new
            {
                voice = $"{ttsConvertOption.Voice.ModelId}:{ttsConvertOption.Voice.Id}",
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

            HttpResponseMessage response = await client.GetAsync(finalUrl, token);

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
            await streamResponse.CopyToAsync(audioToReturn.File);

            return audioToReturn;
        }

        public async Task<List<TtsVoice>> GetTtsVoices(string language)
        {
            //Sending otions 
            var httpResponseMessage = await client.GetFromJsonAsync<Dictionary<string, OpenTtsModelResponse>>($"/api/voices?language={language}");

            var voices = new List<TtsVoice> { };

            if (httpResponseMessage != null)
                foreach (var key in httpResponseMessage?.Keys)
                {
                    voices.Add(new TtsVoice
                    {
                        Id = httpResponseMessage[key].Id,
                        Gender = httpResponseMessage[key].Gender,
                        ModelId = httpResponseMessage[key].tts_name,
                        Language = httpResponseMessage[key].Language,
                        Name = httpResponseMessage[key].Name,
                    });
                }

            return voices;
        }

        public async Task<List<TtsVoice>> GetTtsVoices(string? language, CancellationToken token)
        {
            //Sending otions 
            var httpResponseMessage = await client.GetFromJsonAsync<Dictionary<string, OpenTtsModelResponse>>($"/api/voices?language={language}", token);

            var voices = new List<TtsVoice> { };

            if (httpResponseMessage != null)
                foreach (var key in httpResponseMessage?.Keys)
                {
                    voices.Add(new TtsVoice
                    {
                        Id = httpResponseMessage[key].Id,
                        Gender = httpResponseMessage[key].Gender,
                        ModelId = httpResponseMessage[key].tts_name,
                        Language = httpResponseMessage[key].Language,
                        Name = httpResponseMessage[key].Name,
                    });
                }

            return voices;
        }
    }
}