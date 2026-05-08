using NetXP.IAs.ImageGeneratorAI;

namespace NetXP.ImageGeneratorAI.ComfyBridgeApi
{
    public sealed class ComfyBridgeVideoGeneratorFactory : IVideoGeneratorFactory
    {
        private readonly ComfyBridgeVideoGeneratorClient _videoGenerator;

        public ComfyBridgeVideoGeneratorFactory(ComfyBridgeVideoGeneratorClient videoGenerator)
        {
            _videoGenerator = videoGenerator;
        }

        public IVideoGeneratorAI Resolve(VideoGenerationRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
            return _videoGenerator;
        }
    }
}
