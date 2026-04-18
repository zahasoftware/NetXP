using NetXP.Exceptions;

namespace NetXP.ImageGeneratorAI.ComfyBridgeApi
{
    public class ComfyBridgeJobFailedException : CustomApplicationException
    {
        public ComfyBridgeJobFailedException(string jobId, string? errorMessage)
            : base($"ComfyBridge job '{jobId}' failed: {errorMessage ?? "Unknown error."}")
        {
            JobId = jobId;
            ErrorMessage = errorMessage;
        }

        public string JobId { get; }

        public string? ErrorMessage { get; }
    }
}