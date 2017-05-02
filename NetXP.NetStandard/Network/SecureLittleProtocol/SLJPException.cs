using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    public class SLJPException : Exception
    {
        public SLJPException(string sMsg) : base(sMsg)
        {

        }
    }
}
