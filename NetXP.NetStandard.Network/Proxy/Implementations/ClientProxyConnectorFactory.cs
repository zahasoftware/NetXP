using Microsoft.Extensions.Options;
using NetXP.Auditory;
using NetXP.DependencyInjection;
using NetXP.Network.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetXP.Network.Proxy.Implementations
{
    public class ClientProxyConnectorFactory : IClientConnectorFactory
    {
        private readonly IContainer container;
        private readonly IClientConnectorFactory clientConnectorFactory;
        private readonly ILogger logger;

        public ClientProxyConnectorFactory(
            IContainer container,
            IClientConnectorFactoryProducer clientConnectorFactoryProducer,
            ILogger logger
            )
        {
            this.container = container;
            this.clientConnectorFactory = clientConnectorFactoryProducer.CreateClient(ConnectorFactory.TransmissionControlProtocol);
            this.logger = logger;
        }

        public IClientConnector Create(params object[] @params)
        {
            if (@params.Length != 0)
            {
                var tryTcpClient = @params.SingleOrDefault(o => o is IClientConnector);
                var tcpClient = tryTcpClient as IClientConnector;
                if (tcpClient == null)
                {
                    throw new ArgumentException("Parameter should be of IClientConnector type");
                }

                return new ClientProxyConnector
                (
                    container.Resolve<IClientConnectorFactoryProducer>(),
                    container.Resolve<IOptions<ProxyOptions>>(),
                    logger
                );
            }
            else
            {
                return new ClientProxyConnector
                (
                    container.Resolve<IClientConnectorFactoryProducer>(),
                    container.Resolve<IOptions<ProxyOptions>>(),
                    logger
                );
            }
        }
    }
}
