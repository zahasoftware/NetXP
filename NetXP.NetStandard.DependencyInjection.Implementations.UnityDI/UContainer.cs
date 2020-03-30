using System;
using System.Collections.Generic;
using Unity;

namespace NetXP.NetStandard.DependencyInjection.Implementations.UnityDI
{
    public class UContainer : IContainer
    {
        private readonly IUnityContainer container;

        public UContainer(IUnityContainer container)
        {
            this.container = container;
        }

        public string Name { get; set; }

        public IConfiguration Configuration
        {
            get
            {
                return new UConfiguration(container);
            }
        }

        public void Dispose()
        {
            //this.container.Dispose();
        }

        public TInterface Resolve<TInterface>()
        {
            return this.container.Resolve<TInterface>();
        }

        public TInterface Resolve<TInterface>(string name)
        {
            return this.container.Resolve<TInterface>(name);
        }

        public TInterface Resolve<TInterface>(Action<ICtorInjectorExpression> ctorInjectorExpressionAction)
        {
            //SMCtorInjectorExpression ctorInjectorExpression = new SMCtorInjectorExpression();
            //ctorInjectorExpressionAction(ctorInjectorExpression);
            //foreach (var ctorInjector = ctorInjectorExpression.ctorInjectors)
            //{
            //    this.container.GetInstance<TInterface>();
            //}
            throw new NotImplementedException("Not implemented for Unity.");
        }

        public TInterface Resolve<TInterface>(string name, Action<ICtorInjectorExpression> ctorInjectorExpression)
        {
            throw new NotImplementedException("Not implemented for Unity.");
        }

        public object Resolve(Type interfaceType)
        {
            return this.container.Resolve(interfaceType);
        }

        public IEnumerable<TInterface> ResolveAll<TInterface>()
        {
            return this.container.ResolveAll<TInterface>();
        }
    }
}
