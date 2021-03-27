using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.LittleJsonProtocol.Implementations
{
    internal class LJPDNSCache
    {
        public IPAddress[] Ips { get; internal set; }
        public DateTime Expire { get; internal set; }
        public int Length { get; internal set; }
        public int Current { get; internal set; }
    }
}
