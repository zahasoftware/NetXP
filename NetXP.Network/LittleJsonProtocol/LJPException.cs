using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.LittleJsonProtocol
{
    public enum LJPExceptionType
    {
        BadProtocol = 1,
        NoData = 2,
        FormatException = 3,
        GenericException = 4,
        UnknownException = 5,
        MaxSizeToReceive = 6
    }

    public class LJPException : Exception
    {
        public LJPException(string sMsg) : base(sMsg)
        {
        }

        public LJPException(string sMsg, LJPExceptionType ljpExceptionType, int code = 0, string data = "") : base(sMsg)
        {
            this.LJPExceptionType = ljpExceptionType;
            this.Code = code;
            this.SerializedData = data;
        }

        public LJPExceptionType LJPExceptionType { get; set; }
        public int Code { get; }
        public string SerializedData { get; set; }
    }
}
