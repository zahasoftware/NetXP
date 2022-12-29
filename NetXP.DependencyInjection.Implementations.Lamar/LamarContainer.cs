using Lamar;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace NetXP.DependencyInjection.Implementations.LamarDI
{
    public class LamarContainer : NetXP.DependencyInjection.IContainer
    {
        private readonly ServiceRegistry register;

        public Lamar.IContainer Container { get => (Lamar.Container)ServiceProvider; }

        public LamarContainer(ServiceRegistry register)
        {
            this.register = register;
        }

        public IServiceProvider ServiceProvider { get; set; }

        public string Name { get; set; }

        public IConfiguration Configuration
        {
            get
            {
                return new LamarConfiguration(this.register);
            }
        }


        public void Dispose()
        {
        }

        public TInterface Resolve<TInterface>()
        {
            return this.Container.GetService<TInterface>();
        }

        public TInterface Resolve<TInterface>(string name)
        {
            return this.Container.GetInstance<TInterface>(name);
        }

        public object Resolve(Type interfaceType)
        {
            return this.Container.GetInstance(interfaceType);
        }

        public IEnumerable<TInterface> ResolveAll<TInterface>()
        {
            return this.Container.GetAllInstances<TInterface>();
        }
    }
}
