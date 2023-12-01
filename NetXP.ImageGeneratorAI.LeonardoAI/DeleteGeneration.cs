using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.ImageGeneratorAI.LeonardoAI
{
    public class DeleteGenerationsRequest
    {
        public DeleteGenerationsByPk DeleteGenerationsByPk { get; set; }
    }

    public class DeleteGenerationsByPk
    {
        public Guid Id { get; set; }
    }

}
