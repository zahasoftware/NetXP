using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.TCP.Implementations
{
    public class ServerConnectorFactory : IServerConnectorFactory
    {
        private IContainer container;

        public ServerConnectorFactory(IContainer container)
        {
            this.container = container;
        }

        public IServerConnector Create(ConnectorFactory networkFactory)
        {
            switch (networkFactory)
            {
                case ConnectorFactory.TransmissionControlProtocol:
                    return this.container.Resolve<IServerConnector>("normal");

                case ConnectorFactory.SecureLitleProtocol:
                    return this.container.Resolve<IServerConnector>();

                default: throw new NotImplementedException();
            }
        }

        public IServerConnector Create()
        {
            return this.Create(ConnectorFactory.SecureLitleProtocol);
        }
    }
}
