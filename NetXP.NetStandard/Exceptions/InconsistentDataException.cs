using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Exceptions
{
    public class InconsistentDataException : Exception
    {
        public InconsistentDataException(string msg) :base(msg)
        {

        }
    }
}
