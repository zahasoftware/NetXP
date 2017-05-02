using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Exceptions
{
    public class CustomApplicationException : Exception
    {
        public CustomApplicationException(string msg) : base(msg)
        {

        }
    }
}
