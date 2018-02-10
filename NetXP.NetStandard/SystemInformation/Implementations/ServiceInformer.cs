using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NetXP.NetStandard.Exceptions;
using NetXP.NetStandard.Processes;

namespace NetXP.NetStandard.SystemInformation.Implementations
{
    public class ServiceInformer : IServiceInformer
    {
        private readonly ISystemInformation systemInformation;
        private readonly IIOTerminal terminal;

        public ServiceInformer(
            ISystemInformation systemInformation,
            IIOTerminal terminal
        )
        {
            this.systemInformation = systemInformation;
            this.terminal = terminal;
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
                ProcessOutput output = terminal.Execute(new ProcessInput
                {
                    Command = "systemctl list-unit-files --all --type=service | grep ''",
                    ShellName = "/bin/bash",
                    MaxOfSecondToWaitCommand = 5,
                    Arguments = ""
                });

                foreach (var line in output.StandardOutput.Skip(1))
                {
                    if (!line.Contains("service"))
                    {
                        continue;
                    }
                    var lineSplited = line.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (lineSplited.Length != 2)
                    {
                        throw new SystemInformationException("Service Output Failed, The systemctl Output has changed");
                    }

                    servicesInformations.Add(new ServiceInformation
                    {
                        ServiceName = lineSplited[0]
                    });
                }

            }

            return servicesInformations.Count > 0 ? servicesInformations : null;
        }
    }
}
