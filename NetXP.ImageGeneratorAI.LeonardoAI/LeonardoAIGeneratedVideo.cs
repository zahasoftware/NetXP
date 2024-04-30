using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class LeonardoAIGeneratedVideo
    {
        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("nsfw")]
        public bool? Nsfw { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("likeCount")]
        public int? LikeCount { get; set; }

        [JsonProperty("motionMP4URL")]
        public string? MotionMP4URL { get; set; }

        [JsonProperty("generated_image_variation_generics")]
        public List<object>? GeneratedImageVariationGenerics { get; set; }
    }

    public class LeonardoAIVideoGenerationsByPk
    {
        [JsonProperty("generated_images")]
        public List<LeonardoAIGeneratedVideo>? GeneratedImages { get; set; }

        [JsonProperty("modelId")]
        public object? ModelId { get; set; }

        [JsonProperty("motion")]
        public bool? Motion { get; set; }

        [JsonProperty("motionModel")]
        public string? MotionModel { get; set; }

        [JsonProperty("motionStrength")]
        public int? MotionStrength { get; set; }

        [JsonProperty("prompt")]
        public string? Prompt { get; set; }

        [JsonProperty("negativePrompt")]
        public object? NegativePrompt { get; set; }

        [JsonProperty("imageHeight")]
        public int? ImageHeight { get; set; }

        [JsonProperty("imageToVideo")]
        public bool? ImageToVideo { get; set; }

        [JsonProperty("imageWidth")]
        public int? ImageWidth { get; set; }

        [JsonProperty("inferenceSteps")]
        public object? InferenceSteps { get; set; }

        [JsonProperty("seed")]
        public long? Seed { get; set; }

        [JsonProperty("public")]
        public bool? Public { get; set; }

        [JsonProperty("scheduler")]
        public object? Scheduler { get; set; }

        [JsonProperty("sdVersion")]
        public object? SdVersion { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("presetStyle")]
        public object? PresetStyle { get; set; }

        [JsonProperty("initStrength")]
        public object? InitStrength { get; set; }

        [JsonProperty("guidanceScale")]
        public object? GuidanceScale { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("promptMagic")]
        public bool? PromptMagic { get; set; }

        [JsonProperty("promptMagicVersion")]
        public object? PromptMagicVersion { get; set; }

        [JsonProperty("promptMagicStrength")]
        public object? PromptMagicStrength { get; set; }

        [JsonProperty("photoReal")]
        public object? PhotoReal { get; set; }

        [JsonProperty("photoRealStrength")]
        public object? PhotoRealStrength { get; set; }

        [JsonProperty("fantasyAvatar")]
        public object? FantasyAvatar { get; set; }

        [JsonProperty("generation_elements")]
        public List<object>? GenerationElements { get; set; }
    }

    public class LeonardoAIVideoGeneratedRoot
    {
        [JsonProperty("generations_by_pk")]
        public LeonardoAIVideoGenerationsByPk? GenerationsByPk { get; set; }
    }


}
