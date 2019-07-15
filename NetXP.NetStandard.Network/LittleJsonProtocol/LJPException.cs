using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    public enum LJPExceptionType
    {
        BadProtocol = 1,
        NoData = 2,
        InvalidUser = 3,//Implementation error, 
        RemoteConfigException = 4,
        FormatException = 5,
        GenericException = 6,
        UnknownException = 7,
        MaxSizeToReceive = 8
    }
    public class LJPException : Exception
    {
        public LJPException(string sMsg) : base(sMsg)
        {

        }

        public LJPException(string sMsg, LJPExceptionType nLJPExceptionType, int code = 0) : base(sMsg)
        {
            this.nLJPExceptionType = nLJPExceptionType;
            this.Code = code;
        }

        public LJPExceptionType nLJPExceptionType { get; set; }
        public int Code { get; }
    }
}
