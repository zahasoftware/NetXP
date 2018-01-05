using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Network.TCP.Implementations
{
    /// <summary>
    /// History:
    /// The first implementation of IClientFactory was the "normal" with TCP connection to be used for IClientLJP.
    ///
    /// The second implementation of ICLientFactory was the "sljp" for ICLientLJP to give their secure connections.
    ///
    /// The last implementations of the protocol was the "proxy" for IClientLJP to Manage proxy connections.
    /// 
    /// </summary>
    public class ClientConnectorFactoryProducer : IClientConnectorFactoryProducer
    {
        private readonly IContainer container;

        public ClientConnectorFactoryProducer(IContainer container)
        {
            this.container = container;
        }
        public IClientConnectorFactory CreateClient()
        {
            return this.CreateClient(ConnectorFactory.ProxyProtocol);
        }

        public IClientConnectorFactory CreateClient(ConnectorFactory connectorFactory)
        {
            switch (connectorFactory)
            {
                case ConnectorFactory.ProxyProtocol:
                    return container.Resolve<IClientConnectorFactory>();
                case ConnectorFactory.SecureLitleProtocol:
                    return container.Resolve<IClientConnectorFactory>("secure");
                case ConnectorFactory.TransmissionControlProtocol:
                    return container.Resolve<IClientConnectorFactory>("normal");
                default: throw new NotImplementedException();
            }
        }
    }
}
