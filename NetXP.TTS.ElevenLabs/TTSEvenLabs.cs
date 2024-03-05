using Microsoft.Extensions.Options;
using NetXP.Exceptions;
using NetXP.TTS;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Web;

namespace NetXP.TTSs.ElevenLabs
{
    public class TTSEvenLabs : ITTS
    {
        private readonly IOptions<TTSElevenlabsOptions> options;
        public readonly HttpClient client;

        public TTSEvenLabs(IOptions<TTSElevenlabsOptions> options, IHttpClientFactory clientFactory)
        {
            this.options = options;

            this.client = clientFactory.CreateClient();
            this.client.BaseAddress = new Uri(options.Value.URL);
            this.client.DefaultRequestHeaders.Add("accept", "application/json");
            this.client.DefaultRequestHeaders.Add("xi-api-key", $"{this.options.Value.APIKey}");
        }

        public async Task<TTSAudio> Convert(TTSConvertOption ttsConvertOption)
        {
            if (ttsConvertOption.Voice == null)
            {
                throw new NetXP.Exceptions.CustomApplicationException("Voice cannot be null in TTSConvertOption");
            }

            //Query parameters
            var body = new
            {
                text = ttsConvertOption.Text,
                model_id = ttsConvertOption.Voice.ModelId,
                voice_settings = new
                {
                    stability = 0.5,
                    similarity_boost = 0.5,
                    style = 0.5,
                    use_speaker_boost = true
                }
            };

            string json = JsonConvert.SerializeObject(body);   //using Newtonsoft.Json
            StringContent httpContent = new(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(
                  string.Format(this.options.Value.TextToSpeechUri, ttsConvertOption.Voice.Id)
                , httpContent);

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

        public async Task<List<TTSVoice>> GetTTSVoices(string language = null)
        {
            var finalUrl = this.options.Value.GetVoicesUri;

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
            var result = await response.Content.ReadFromJsonAsync<Root>();

            var ttsVoices = new List<TTSVoice>();
            foreach (var v in result.voices)
            {
                ttsVoices.Add(new TTSVoice()
                {
                    Id = v.voice_id,
                    Gender = v.labels.gender,
                    Language = null,
                    Name = v.name,
                    ModelId = v.high_quality_base_model_ids.FirstOrDefault(),
                    Tags = $"{v.category} {v.labels.use_case}",
                });
            }

            return ttsVoices;
        }

        public async Task<List<TTSVoice>> GetTTSVoices(string language, CancellationToken token)
        {
            var finalUrl = this.options.Value.GetVoicesUri;

            HttpResponseMessage response = await client.GetAsync(finalUrl, token);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error when trying to get voice {await response.Content.ReadAsStringAsync(token)}");
            }

            var ttsVoices = new List<TTSVoice>();
            if (!token.IsCancellationRequested)
            {
                var audioToReturn = new TTSAudio()
                {
                    Format = response?.Content?.Headers?.ContentType?.MediaType,
                    File = new MemoryStream()
                };
                var result = await response.Content.ReadFromJsonAsync<Root>();

                foreach (var v in result.voices)
                {
                    ttsVoices.Add(new TTSVoice()
                    {
                        Id = v.voice_id,
                        Gender = v.labels.gender,
                        Language = null,
                        Name = v.name,
                        ModelId = v.high_quality_base_model_ids.FirstOrDefault(),
                        Tags = $"{v.category} {v.labels.use_case}",
                    });
                }

            }
            return ttsVoices;
        }

        public async Task<TTSAudio> Convert(TTSConvertOption ttsConvertOption, CancellationToken token)
        {
            if (ttsConvertOption.Voice == null)
            {
                throw new NetXP.Exceptions.CustomApplicationException("Voice cannot be null in TTSConvertOption");
            }

            //Query parameters
            var body = new
            {
                text = ttsConvertOption.Text,
                model_id = ttsConvertOption.Voice.ModelId,
                voice_settings = new
                {
                    stability = 0.5,
                    similarity_boost = 0.5,
                    style = 0.5,
                    use_speaker_boost = true
                }
            };

            string json = JsonConvert.SerializeObject(body);   //using Newtonsoft.Json
            StringContent httpContent = new(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(
                  string.Format(this.options.Value.TextToSpeechUri, ttsConvertOption.Voice.Id)
                , httpContent, token);

            if (token.IsCancellationRequested)
            {
                throw new CustomApplicationException("Operation Cancelled by User");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"{await response.Content.ReadAsStringAsync()}");
            }

            var audioToReturn = new TTSAudio()
            {
                Format = response?.Content?.Headers?.ContentType?.MediaType,
                File = new MemoryStream()
            };
            var streamResponse = await response.Content.ReadAsStreamAsync(token);
            await streamResponse.CopyToAsync(audioToReturn.File);

            return audioToReturn;
        }
    }
}