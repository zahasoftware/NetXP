using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace NetXP.NetStandard.SystemInformation.Implementations
{
    public class ServiceInformer : IServiceInformer
    {
        private readonly ISystemInformation systemInformation;

        public ServiceInformer(ISystemInformation systemInformation)
        {
            this.systemInformation = systemInformation;
        }

        public List<ServiceInformation> GetServices()
        {
            List<ServiceInformation> servicesInformations = new List<ServiceInformation>();
            var osInfo = systemInformation.GetOSInfo();
            //Window 
            if (osInfo.Platform == OSPlatformType.Windows)
            {
                var services = ServiceController.GetServices();
                foreach (var service in services)
                {
                    servicesInformations.Add(new ServiceInformation
                    {
                        ServiceName = service.ServiceName
                    });
                }
            }
            else if (osInfo.Platform == OSPlatformType.Linux)
            {
                throw new NotImplementedException();
            }

            return servicesInformations.Count > 0 ? servicesInformations : null;
        }
    }
}
