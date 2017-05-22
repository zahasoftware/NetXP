using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Network.TCP
{
    public interface IClientConnectorFactoryProducer
    {
        IClientConnectorFactory CreateClient(ConnectorFactory connectorFactory);
        IClientConnectorFactory CreateClient();
    }
}
