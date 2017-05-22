using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    public class SLPException : Exception
    {
        public SLPException(string sMsg) : base(sMsg)
        {

        }
    }
}
