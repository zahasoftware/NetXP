using System.Text.Json.Serialization;

namespace NetXP.Tts.ChatterboxApi
{
    internal sealed class ChatterboxHealthResponse
    {
        public string? status { get; set; }
        public bool cuda_available { get; set; }
        public string? device { get; set; }
        public string? default_device { get; set; }
    }

    internal sealed class ChatterboxVoicesResponse
    {
        public int count { get; set; }
        public List<ChatterboxVoiceItem> voices { get; set; } = [];
    }

    internal sealed class ChatterboxVoiceItem
    {
        public string? id { get; set; }
        public string? filename { get; set; }
        public string? relative_path { get; set; }
    }

    internal sealed class ChatterboxLanguagesResponse
    {
        public int count { get; set; }
        public Dictionary<string, string> languages { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    internal sealed class ChatterboxGenerateRequest
    {
        public string model { get; set; } = "turbo";
        public string device { get; set; } = "cpu";
        public string text { get; set; } = string.Empty;

        [JsonPropertyName("voice_id")]
        public string? voice_id { get; set; }

        [JsonPropertyName("language_id")]
        public string? language_id { get; set; }

        public double temperature { get; set; } = 0.8;

        [JsonPropertyName("top_p")]
        public double top_p { get; set; } = 0.95;
    }
}
