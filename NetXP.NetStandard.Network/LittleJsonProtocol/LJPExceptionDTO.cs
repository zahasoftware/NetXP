﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NetXP.NetStandard.Network.LittleJsonProtocol;

namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    [DataContract]
    public class LJPExceptionDTO
    {

        public LJPExceptionDTO()
        { }
        public LJPExceptionDTO(string message, LJPExceptionType genericException)
        {
            this.Message = message;
            this.IClientLJPExceptionType = (int)genericException;
        }

        [DataMember]
        public int IClientLJPExceptionType { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public int Code { get; set; }

        [DataMember]
        public string SerializedData { get; set; }

    }
}
