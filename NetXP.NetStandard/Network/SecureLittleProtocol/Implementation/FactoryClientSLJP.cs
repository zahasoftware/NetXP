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
    public class FactoryClientSLJP : IFactoryClientTCP
    {
        private IContainer oIUC;
        public FactoryClientSLJP(IContainer oIUC)
        {
            this.oIUC = oIUC;
        }

        public ITCPClient Create(params object[] aParams)
        {
            if (aParams.Count() > 0)
            {
                var tryTcpClient = aParams.SingleOrDefault(o => o is ITCPClient);
                var tcpClient = tryTcpClient as ITCPClient;

                return oIUC.Resolve<ITCPClient>(
                    ctor =>
                    {
                        ctor.InjectInstance("client", tcpClient);
                    });
            }
            else
            {
                return this.oIUC.Resolve<ITCPClient>();
            }
        }
    }
}
