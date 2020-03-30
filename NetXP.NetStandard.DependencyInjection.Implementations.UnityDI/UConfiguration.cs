using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace NetXP.NetStandard.DependencyInjection.Implementations.UnityDI
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
            var c = new URegisterExpression(container);
            expression(c);
        }
    }
}
