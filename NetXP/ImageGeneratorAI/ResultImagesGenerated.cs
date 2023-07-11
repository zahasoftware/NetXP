using System.Collections.Generic;

namespace NetXP.ImageGeneratorAI
{
    public class ResultImagesGenerated
    {
        public ResultImagesGenerated()
        {
            this.Images = new List<ImageGenerate>();
        }
        public List<ImageGenerate> Images { get; set; }
    }
}