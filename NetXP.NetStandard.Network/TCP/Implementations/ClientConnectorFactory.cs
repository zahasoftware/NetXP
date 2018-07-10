using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.TCP.Implementations
{
    public class ClientConnectorFactory : IClientConnectorFactory
    {
        private IContainer oIUC;

        public ClientConnectorFactory(IContainer oIUC)
        {
            this.oIUC = oIUC;
        }

        public IClientConnector Create(object[] aParams)
        {
            if (aParams.Length != 0)
            {
                var trySocket = aParams.SingleOrDefault(o => o is Socket);
                var socket = trySocket as Socket;

                if (socket == null)
                    throw new ArgumentException("Only one socket is supported in constructor parameter.");

                return new SocketClientConnector(socket);
            }
            else
            {
                return this.oIUC.Resolve<IClientConnector>("normal");
            }
        }
    }
}
