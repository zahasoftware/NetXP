using System;
using System.Collections.Generic;
using System.Text;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.SystemInformation;

namespace NetXP.NetStandard.SystemManagers.Implementations
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
