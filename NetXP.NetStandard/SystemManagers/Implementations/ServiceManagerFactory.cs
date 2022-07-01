using System;
using System.Collections.Generic;
using System.Text;
using NetXP.DependencyInjection;
using NetXP.SystemInformation;

namespace NetXP.SystemManagers.Implementations
{
    public class ServiceManagerFactory : IServiceManagerFactory
    {
        private readonly IContainer container;

        public ServiceManagerFactory(IContainer container)
        {
            this.container = container;
        }

        public IServiceManager Create(OSPlatformType osPlatformType)
        {
            if (osPlatformType == OSPlatformType.Linux
                || osPlatformType == OSPlatformType.Windows)
            {
                return container.Resolve<IServiceManager>(osPlatformType.ToString());
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}
