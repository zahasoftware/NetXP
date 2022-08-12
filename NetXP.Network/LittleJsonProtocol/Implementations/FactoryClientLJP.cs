using NetXP.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.LittleJsonProtocol.Implementations
{
    public class FactoryClientLJP : IFactoryClientLJP
    {
        private readonly IContainer uc;

        public FactoryClientLJP(IContainer oIUnityContainer)
        {
            this.uc = oIUnityContainer;
        }

        public IClientLJP Create()
        {
            return this.uc.Resolve<IClientLJP>();
        }

        public ILJPMessageFactory CreateMessageFactory(string version)
        {
            return this.uc.Resolve<ILJPMessageFactory>(version);
        }

    }
}
