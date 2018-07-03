using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace NetXP.NetStandard.DependencyInjection.Implementations.DIUnity
{
    public class UConfiguration : IConfiguration
    {
        private readonly IUnityContainer container;

        public UConfiguration(IUnityContainer container)
        {
            this.container = container;
        }

        public void Configure(Action<IRegister> expression)
        {
            expression(new URegisterExpression( container));
        }
    }
}
