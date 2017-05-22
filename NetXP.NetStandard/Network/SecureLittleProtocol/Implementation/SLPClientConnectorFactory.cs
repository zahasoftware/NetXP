using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.Cryptography.Implementations;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Network.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.SecureLittleProtocol.Implementation
{
    public class SLPClientConnectorFactory : IClientConnectorFactory
    {
        private IContainer container;
        public SLPClientConnectorFactory(IContainer container)
        {
            this.container = container;
        }

        public IClientConnector Create(params object[] @params)
        {
            if (@params.Count() > 0)
            {
                var tryTcpClient = @params.SingleOrDefault(o => o is IClientConnector);
                var tcpClient = tryTcpClient as IClientConnector;

                return new SLPClientConnector(
                    tcpClient
                    );
            }
            else
            {
                return this.container.Resolve<IClientConnector>();
            }
        }
    }
}
