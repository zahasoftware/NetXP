using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json;
using NetXP.IAs.Chat;
using NetXP.Exceptions;

namespace NetXP.IAs.Chats.Ollama
{
    public class OllamaChatService : IAIChatService
    {
        private readonly HttpClient _httpClient;
        private readonly AIChatConfig _config;
        private string _currentModelId;

        public OllamaChatService(HttpClient httpClient, IOptions<AIChatConfig> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
        }

        public async Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var request = new
            {
                model = _currentModelId,
                prompt = prompt,
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync($"{_config.BaseUrl}/{_config.GenerateEndpoint}", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
            return result?.Response ?? string.Empty;
        }

        public async Task<string> GenerateResponseAsync(string prompt, IDictionary<string, string> parameters, CancellationToken cancellationToken = default)
        {
            var request = new
            {
                modelId = _currentModelId,
                prompt = prompt,
                parameters = parameters
            };

            var response = await _httpClient.PostAsJsonAsync($"{_config.BaseUrl}/{_config.GenerateEndpoint}", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
            return result?.Response ?? string.Empty;
        }

        public async Task<List<ChatIAModelResponse>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"{_config.BaseUrl}/{_config.ModelsEndpoint}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponseModels>(cancellationToken: cancellationToken);

            if (result == null || result.Models == null || result.Models.Count == 0)
            {
                throw new NetXPApplicationException($"No models available {_config.BaseUrl}/{_config.ModelsEndpoint}");
            }

            return result.Models.Select(o => new ChatIAModelResponse
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
                ModifiedAt = o.ModifiedAt,
                Size = o.Size,
            }).ToList() ?? [];
        }

        public async Task SetModelAsync(string modelId, CancellationToken cancellationToken = default)
        {
            _currentModelId = modelId;
            await Task.CompletedTask;
        }

        public class OllamaResponse
        {
            public string Response { get; set; }
        }

        public class OllamaResponseModels
        {
            public List<OllamaModel> Models { get; set; }
        }

        public class OllamaModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime ModifiedAt { get; set; }
            public long Size { get; set; }
        }
    }
}
