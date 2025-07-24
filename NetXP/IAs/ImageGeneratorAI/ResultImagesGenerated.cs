using System.Collections.Generic;

namespace NetXP.IAs.ImageGeneratorAI
{
    public class ResultImagesGenerated
    {
        public ResultImagesGenerated()
        {
            Images = new List<ImageGenerate>();
        }
        public List<ImageGenerate> Images { get; set; }
    }
}