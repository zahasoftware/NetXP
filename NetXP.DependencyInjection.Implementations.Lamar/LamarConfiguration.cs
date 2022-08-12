using Lamar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.DependencyInjection.Implementations.LamarDI
{
    public class LamarConfiguration : IConfiguration
    {
        private readonly ServiceRegistry register;

        public LamarConfiguration(Lamar.ServiceRegistry register)
        {
            this.register = register;
        }

        public void Configure(Action<IRegister> expression)
        {
            expression(new LamarRegisterExpression(register));
        }
    }
}
