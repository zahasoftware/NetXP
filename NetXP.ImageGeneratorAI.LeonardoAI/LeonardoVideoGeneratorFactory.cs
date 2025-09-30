using Microsoft.Extensions.Options;
using NetXP.IAs.ImageGeneratorAI;

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    public class LeonardoVideoGeneratorFactory : IVideoGeneratorFactory
    {
        private readonly LeonardoMotion1VideoGenerator _motion1;
        private readonly LeonardoMotion2VideoGenerator _motion2;

        public LeonardoVideoGeneratorFactory(
            IHttpClientFactory factory,
            IOptions<ImageGeneratorAIOptions> options)
        {
            _motion1 = new LeonardoMotion1VideoGenerator(factory, options);
            _motion2 = new LeonardoMotion2VideoGenerator(factory, options);
        }

        public IVideoGeneratorAI Resolve(VideoGenerationRequest request) =>
            request.Version switch
            {
                MotionVersion.Motion2 => _motion2,
                _ => _motion1
            };
    }
}