using NetXP.NetStandard.Network.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.LittleJsonProtocol.Implementations
{
    /// <summary>
    /// Service Manager Protocol for "Little Json Protocol"
    /// </summary>
    public class ServerLJP : IServerLJP
    {
        private readonly IServerConnector oIServerTCP;
        private readonly IFactoryClientLJP factoryClientLJP;

        public ServerLJP(IServerConnector serverTCP
            , IFactoryClientLJP factoryClientTCP
            )
        {
            this.oIServerTCP = serverTCP;
            this.factoryClientLJP = factoryClientTCP;

        }

        public void Listen(System.Net.IPAddress oIPAddress, int iPort)
        {
            this.oIServerTCP.Listen(oIPAddress, iPort);
        }

        public async Task<IClientLJP> Accept()
        {
            IClientLJP clientJLP = this.factoryClientLJP.Create();
            clientJLP.ClientTCP = await this.oIServerTCP.Accept();
            return clientJLP;
        }
    }
}
