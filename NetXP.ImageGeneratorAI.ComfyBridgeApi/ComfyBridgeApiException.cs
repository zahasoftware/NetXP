using NetXP.Exceptions;
using System.Net;

namespace NetXP.ImageGeneratorAI.ComfyBridgeApi
{
    public class ComfyBridgeApiException : CustomApplicationException
    {
        public ComfyBridgeApiException(string message, HttpStatusCode statusCode, string? responseBody)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public HttpStatusCode StatusCode { get; }

        public string? ResponseBody { get; }
    }
}