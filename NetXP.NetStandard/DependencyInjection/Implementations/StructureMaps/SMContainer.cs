using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StructureMap;
using NetXP.NetStandard.Cryptography;

namespace NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps
{
    public class SMContainer : IContainer
    {
        private readonly StructureMap.Container container;

        public SMContainer(StructureMap.Container container)
        {
            this.container = container;
        }

        public string Name { get; set; }

        public IConfiguration Configuration
        {
            get
            {
                return new SMConfiguration(container);
            }
        }

        public void Dispose()
        {
            this.container.Dispose();
        }

        public TInterface Resolve<TInterface>()
        {
            return this.container.GetInstance<TInterface>();
        }

        public TInterface Resolve<TInterface>(string name)
        {
            return this.container.GetInstance<TInterface>(name);
        }

        public TInterface Resolve<TInterface>(Action<ICtorInjectorExpression> ctorInjectorExpressionAction)
        {
            //SMCtorInjectorExpression ctorInjectorExpression = new SMCtorInjectorExpression();
            //ctorInjectorExpressionAction(ctorInjectorExpression);
            //foreach (var ctorInjector = ctorInjectorExpression.ctorInjectors)
            //{
            //    this.container.GetInstance<TInterface>();
            //}
            throw new NotImplementedException();
        }

        public TInterface Resolve<TInterface>(string name, Action<ICtorInjectorExpression> ctorInjectorExpression)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type interfaceType)
        {
            throw new NotImplementedException();
        }
    }
}
