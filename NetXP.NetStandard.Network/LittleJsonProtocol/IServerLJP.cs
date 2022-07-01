using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NetXP.NetStandard;
using System.Net;
using System.Threading.Tasks;

namespace NetXP.Network.LittleJsonProtocol
{
    public interface IServerLJP
    {
        void Listen(IPAddress oIPAddress, int iPort);
        Task<IClientLJP> Accept();

    }
}
