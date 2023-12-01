using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    public class SdGenerationJobRoot
    {
        public SdGenerationJob SdGenerationJob { get; set; }
    }

    public class SdGenerationJob
    {
        public string GenerationId { get; set; }
    }
}
