using System;

namespace NetXP.ImageGeneratorAI.ComfyBridgeApi
{
    public class ComfyBridgeClientOptions
    {
        public const string SectionName = "ComfyBridge";

        public string BaseUrl { get; set; } = "http://localhost:8188";
        public int PollIntervalMs { get; set; } = 1000;
        public int JobTimeoutSeconds { get; set; } = 120;
        public string? TemplateName { get; set; }
        public string? Category { get; set; }

        public TimeSpan GetPollInterval()
        {
            return TimeSpan.FromMilliseconds(Math.Max(100, PollIntervalMs));
        }

        public TimeSpan GetTimeout()
        {
            return TimeSpan.FromSeconds(Math.Max(1, JobTimeoutSeconds));
        }
    }
}