using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.LittleJsonProtocol
{
    public class LJPCallMessageDTO
    {
        public LJPCredentialDTO Credential { get; set; }
        public string [] Parameters { get; set; }
    }
}
