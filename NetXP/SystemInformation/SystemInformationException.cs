using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Exceptions
{
    public class SystemInformationException: Exception
    {
        public SystemInformationException(string msg) : base(msg)
        {

        }
    }
}

