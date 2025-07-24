using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetXP.IAs.Chat
{
    public interface IAIChatService
    {
        Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);
        Task<string> GenerateResponseAsync(string prompt, IDictionary<string, string> parameters, CancellationToken cancellationToken = default);
        Task<List<ChatIAModelResponse>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
        Task SetModelAsync(string modelId, CancellationToken cancellationToken = default);
    }
}