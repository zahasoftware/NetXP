using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    /// <summary>
    /// Sended object
    /// </summary>
    public class LJPCall : LJPCallHeader
    {
        public LJPCall()
        {
            this.Version = "1.0";
        }

        public string NameInterface { get; set; }
        public string NameMethod { get; set; }
        
    }
}
