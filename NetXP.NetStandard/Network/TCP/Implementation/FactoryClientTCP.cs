using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.TCP.Implementation
{
    public class FactoryClientTCP : IFactoryClientTCP
    {
        private IContainer oIUC;

        public FactoryClientTCP(IContainer oIUC)
        {
            this.oIUC = oIUC;
        }

        public ITCPClient Create(params object[] aParams)
        {
            if (aParams.Length != 0)
            {
                var trySocket = aParams.SingleOrDefault(o => o is Socket);
                var socket = trySocket as Socket;

                if (socket == null)
                    throw new ArgumentException("Only one socket is supported in constructor parameter.");

                return this.oIUC.Resolve<ITCPClient>("normal-socket",
                    ctor =>
                    {
                        ctor.InjectInstance("socket", socket);
                    });
            }
            else
            {
                return this.oIUC.Resolve<ITCPClient>("normal");
            }
        }
    }
}
