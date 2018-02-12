using NetXP.NetStandard.Exceptions;
using NetXP.NetStandard.SystemInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SmartSecurity.NetStandard.NetFramework.SystemInformation.Implementations
{
    public class ServiceInformerWindowsOnly : IServiceInformer
    {

        /// <summary>
        /// Display the list of services currently running on this computer.
        /// </summary>
        /// <returns>List with service information.</returns>
        public List<ServiceInformation> GetServices()
        {
            List<ServiceInformation> servicesInformations = new List<ServiceInformation>();
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();


            var services = ServiceController.GetServices();
            foreach (var service in services)
            {
                // Query WMI for additional information about this service.
                // Display the start name (LocalSytem, etc) and the service
                // description.
                ManagementObject wmiService;
                wmiService = new ManagementObject("Win32_Service.Name='" + service.ServiceName + "'");
                wmiService.Get();

                var serviceInformation = new ServiceInformation
                {
                    ServiceName = service.ServiceName,
                    Description = wmiService["Description"]?.ToString(),
                    State = service.Status == ServiceControllerStatus.Running ? ServiceState.Running :
                                                service.Status == ServiceControllerStatus.Stopped ? ServiceState.Stopped : ServiceState.Unknown,

                    StartupState = service.StartType == ServiceStartMode.Automatic
                                || service.StartType == ServiceStartMode.Boot
                                ? ServiceStartupState.Active
                                /*else if*/: service.StartType == ServiceStartMode.Disabled
                                || service.StartType == ServiceStartMode.Manual
                                || service.StartType == ServiceStartMode.System
                                ? ServiceStartupState.Disabled
                                /*else*/ : throw new SystemInformationException("Cannot Determine Service Startup State")

                };

                servicesInformations.Add(serviceInformation);

            }

            return servicesInformations.Count > 0 ? servicesInformations : null;
        }
    }
}
