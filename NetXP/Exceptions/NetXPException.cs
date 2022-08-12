using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Exceptions
{
    public class NetXPApplicationException : ApplicationException
    {
        public NetXPApplicationException(string msg) : base(msg)
        {

        }
    }
}
