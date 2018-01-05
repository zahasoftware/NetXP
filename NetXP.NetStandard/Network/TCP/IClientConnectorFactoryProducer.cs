using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Network.TCP
{
    /// <summary>
    /// Producer to avoid get factory by specified attribute of any dependency injector like Unity that use DependecyAttribute to 
    /// get implementation by name.
    /// </summary>
    public interface IClientConnectorFactoryProducer
    {
        IClientConnectorFactory CreateClient(ConnectorFactory connectorFactory);
        IClientConnectorFactory CreateClient();
    }
}
