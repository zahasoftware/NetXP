using System;
using System.Collections.Generic;

namespace NetXP.IAs.ImageGeneratorAI
{
    public class ImageGeneratorAIOptions
    {
        public string Token { get; set; }
        public Dictionary<string, object?> ExtraOptions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
