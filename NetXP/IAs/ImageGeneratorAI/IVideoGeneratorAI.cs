using System.Threading;
using System.Threading.Tasks;

namespace NetXP.IAs.ImageGeneratorAI
{
    public enum MotionVersion
    {
        Motion1,   // Current SVD (generations-motion-svd)
        Motion2    // Motion 2.0 (image-based)
    }

    // Generic request covering both motion versions
    public class VideoGenerationRequest
    {
        // Either SourceImagePath (local file to upload) or SourceImageId (already uploaded image)
        public string? SourceImagePath { get; set; }
        public string? SourceImageId { get; set; }

        // Motion 2 might use prompt + optional negative prompt
        public string? Prompt { get; set; }

        public MotionVersion Version { get; set; } = MotionVersion.Motion2;

        // Shared / optional
        public int? MotionStrength { get; set; } = 5;
        public string? Variant { get; set; } = "MOTION2";        // For Motion2: MOTION_2 or MOTION_2_FAST
        public int? Fps { get; set; }
        public object? ExtraOptions { get; set; }
        public int Height { get; set; } = 832;
        public int Width { get; set; } = 480;
    }

    public interface IVideoGeneratorAI
    {
        Task<VideoGenerated> GenerateVideoAsync(VideoGenerationRequest request, CancellationToken token = default);
    }

    public interface IVideoGeneratorFactory
    {
        IVideoGeneratorAI Resolve(VideoGenerationRequest request);
    }
}