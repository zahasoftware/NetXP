using NetXP.NetStandard.DependencyInjection;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps
{
    public class SMConfiguration : IConfiguration
    {
        private readonly Container container;

        public SMConfiguration(Container container)
        {
            this.container = container;
        }

        public void Configure(Action<IRegister> expression)
        {
            this.container.Configure(cnf =>
            {
                expression(new SMRegisterExpression(cnf,container));
            });
        }
    }
}
