using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    //Little Json Protocol Header
    //All package receive this information in the network.
    public abstract class LJPHeader
    {
        public int iLength { get; set; }
        
    }
    
}
