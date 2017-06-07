using System;

namespace NetXP.NetStandard.Exceptions
{
    public class SecurityException : Exception
    {
        public SecurityExceptionCode SecurityCode { get; set; }

        public SecurityException(string message, SecurityExceptionCode securityCode) : base(message)
        {
            this.SecurityCode = securityCode;
        }
    }
}
