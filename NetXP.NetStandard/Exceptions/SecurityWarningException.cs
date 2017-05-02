using NetXP.NetStandard.Exceptions.enm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Exceptions
{
    public class SecurityException : Exception
    {
        public enm.SecurityExceptionCode SecurityCode { get; set; }

        public SecurityException(string message, SecurityExceptionCode securityCode) : base(message)
        {
            this.SecurityCode = securityCode;
        }
    }
}
