using Lamar;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace NetXP.NetStandard.DependencyInjection.Implementations.LamarDI
{
    public class LamarContainer : NetXP.NetStandard.DependencyInjection.IContainer
    {
        private readonly ServiceRegistry register;

        public IServiceProvider ServiceProvider { get; set; }

        public LamarContainer(ServiceRegistry register, IServiceProvider serviceProvider)
        {
            this.register = register;
            this.ServiceProvider = serviceProvider?.CreateScope()?.ServiceProvider;
        }

        public string Name { get; set; }

        public IConfiguration Configuration
        {
            get
            {
                return new LamarConfiguration(this.register);
            }
        }


        private Container container { get => this.ServiceProvider as Container; }

        public void Dispose()
        {
        }

        public TInterface Resolve<TInterface>()
        {
            return this.container.GetService<TInterface>();
        }

        public TInterface Resolve<TInterface>(string name)
        {
            return this.container.GetInstance<TInterface>(name);
        }

        public object Resolve(Type interfaceType)
        {
            return this.container.GetInstance(interfaceType);
        }

        public IEnumerable<TInterface> ResolveAll<TInterface>()
        {
            return this.container.GetAllInstances<TInterface>();
        }
    }
}
