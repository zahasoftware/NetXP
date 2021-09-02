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
            ///Version used to change some implementation, used in SendCall Implementations
            this.Version = "2.0";
        }

        public string InterfaceName { get; set; }
        public string MethodName { get; set; }

    }
}
