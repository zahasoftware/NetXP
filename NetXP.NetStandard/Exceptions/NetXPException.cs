using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Exceptions
{
    public class NetXPApplicationException : Exception
    {
        public NetXPApplicationException(string msg) : base(msg)
        {

        }
    }
}
