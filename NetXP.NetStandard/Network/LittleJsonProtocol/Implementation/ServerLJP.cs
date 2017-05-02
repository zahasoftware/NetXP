using NetXP.NetStandard.Network.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.LittleJsonProtocol.Implementation
{
    /// <summary>
    /// Service Manager Protocol for "Little Json Protocol"
    /// </summary>
    public class ServerLJP : IServerLJP
    {
        private readonly IServerTCP oIServerTCP;
        private readonly IFactoryClientLJP oIFactoryClientLJP;

        public ServerLJP(IServerTCP serverTCP
            , IFactoryClientLJP factoryClientTCP
            )
        {
            this.oIServerTCP = serverTCP;
            this.oIFactoryClientLJP = factoryClientTCP;

        }

        public void Listen(System.Net.IPAddress oIPAddress, int iPort)
        {
            this.oIServerTCP.Listen(oIPAddress, iPort);
        }

        public async Task<IClientLJP> Accept()
        {
            IClientLJP oIClientJLP = this.oIFactoryClientLJP.Create();
            oIClientJLP.oIClientTCP = await this.oIServerTCP.Accept();
            return oIClientJLP;
        }
    }
}
