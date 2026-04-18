using Microsoft.Extensions.Options;
using NetXP;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Translators.OllamaTranslator
{
    public class OllamaTranslatorImplementation : ITranslator
    {
        private readonly IOptions<OllamaTranslatorOptions> _options;
        private readonly HttpClient _httpClient;

        public OllamaTranslatorImplementation(IOptions<OllamaTranslatorOptions> options, HttpClient httpClient)
        {
            _options = options;
            _httpClient = httpClient;

            if (string.IsNullOrEmpty(_options.Value.BaseUrl))
            {
                throw new ArgumentException("Ollama Translator BaseUrl is not set.");
            }

            if (string.IsNullOrEmpty(_options.Value.DefaultModel))
            {
                throw new ArgumentException("Ollama Translator DefaultModel is not set.");
            }
        }

        public Task<string> TranslateTextAsync(string text, string toLanguage)
        {
            var defaultInstruction = $"Translate the following text to {toLanguage}. Return only the translated text without any additional explanation or formatting.";
            return TranslateTextAsync(text, toLanguage, defaultInstruction);
        }

        public async Task<string> TranslateTextAsync(string text, string toLanguage, string instruction)
        {
            var prompt = $"{instruction}\n\nText to translate to {toLanguage}:\n{text}";

            var requestBody = new
            {
                model = _options.Value.DefaultModel,
                prompt = prompt,
                stream = false
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var endpoint = $"{_options.Value.BaseUrl.TrimEnd('/')}/{_options.Value.GenerateEndpoint}";
            var response = await _httpClient.PostAsync(endpoint, content);
            
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonConvert.DeserializeObject<OllamaResponse>(responseContent);

            if (ollamaResponse?.Response == null)
            {
                throw new InvalidOperationException("Ollama API returned an empty response.");
            }

            return ollamaResponse.Response.Trim();
        }

        private class OllamaResponse
        {
            [JsonProperty("response")]
            public string? Response { get; set; }
        }
    }
}
