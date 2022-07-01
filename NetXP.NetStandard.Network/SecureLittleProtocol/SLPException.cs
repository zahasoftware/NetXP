using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.LittleJsonProtocol
{
    public class SLPException : Exception
    {
        public SLPExceptionType ExceptionType { get; }

        public enum SLPExceptionType
        {
            BadProtocol = 1,
            NoDataTimeOut = 2,
            FormatException = 5,
            GenericException = 6,
            UnknownException = 7,
            MaxSizeToReceive = 8,
            PPKNotFound = 9
        }

        public SLPException(string message) : base(message)
        {

        }

        public SLPException(string message, SLPExceptionType exceptionType) : base(message)
        {
            this.ExceptionType = exceptionType;
        }
    }
}
