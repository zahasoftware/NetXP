using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Network.TCP.Implementations
{
    [Obsolete("Don't use, Delete this")]
    public class ClientConnectorFactoryProducer : IClientConnectorFactoryProducer
    {
        private readonly IContainer container;

        public ClientConnectorFactoryProducer(IContainer container)
        {
            this.container = container;
        }
        public IClientConnectorFactory CreateClient()
        {
            return this.CreateClient(ConnectorFactory.SecureLitleProtocol);
        }

        public IClientConnectorFactory CreateClient(ConnectorFactory connectorFactory)
        {
            switch (connectorFactory)
            {
                case ConnectorFactory.SecureLitleProtocol:
                    return container.Resolve<IClientConnectorFactory>();
                case ConnectorFactory.TransmissionControlProtocol:
                    return container.Resolve<IClientConnectorFactory>("normal");
                default: throw new NotImplementedException();
            }
        }
    }
}
