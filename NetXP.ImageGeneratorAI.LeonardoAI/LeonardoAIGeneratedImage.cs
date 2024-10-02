using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    public class GeneratedImageLoneardoAI
    {
        public string? url { get; set; }
        public bool? nsfw { get; set; }
        public string? id { get; set; }
        public int? likeCount { get; set; }
        public object[]? generated_image_variation_generics { get; set; }
    }

    public class GenerationsByPkLeonardoAI
    {
        public GeneratedImageLoneardoAI[] generated_images { get; set; }
        public string? modelId { get; set; }
        public string?    prompt { get; set; }
        public string? negativePrompt { get; set; }
        public long imageHeight { get; set; }
        public long imageWidth { get; set; }
        public long inferenceSteps { get; set; }
        public long? seed { get; set; }
        public bool @public { get; set; }
        public string? scheduler { get; set; }
        public string? sdVersion { get; set; }
        public string? status { get; set; }
        public object? presetStyle { get; set; }
        public object? initStrength { get; set; }
        public long guidanceScale { get; set; }
        public string? id { get; set; }
        public DateTime? createdAt { get; set; }
    }

    public class GenerateImageLeonardoAIRoot
    {
        public GenerationsByPkLeonardoAI generations_by_pk { get; set; }
    }
}
