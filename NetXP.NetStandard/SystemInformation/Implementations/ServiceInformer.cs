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
                throw new PlatformNotSupportedException("Cannot get all state of service with Net Core, Please use NetFramework implementation for windows system.");

            
            }
            else if (osInfo.Platform == OSPlatformType.Linux)
            {
                ProcessOutput output = terminal.Execute(new ProcessInput
                {
                    Command = "systemctl list-units --type=service | grep ''",
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
                    var lineSplited = line.Split(new char[] { ' ' }, 5, StringSplitOptions.RemoveEmptyEntries);
                    if (lineSplited.Length != 5)
                    {
                        throw new SystemInformationException("Service Output Failed, The systemctl Output has changed");
                    }

                    servicesInformations.Add(new ServiceInformation
                    {
                        ServiceName = lineSplited[0],
                        Description = lineSplited[4],
                        State = GetServiceState(OSPlatformType.Linux, lineSplited),
                        StartupState = GetServiceStartupState(OSPlatformType.Linux, lineSplited)
                    });
                }

            }

            return servicesInformations.Count > 0 ? servicesInformations : null;
        }

        private ServiceStartupState GetServiceStartupState(OSPlatformType osPlatform, string[] lineSplited)
        {
            if (osPlatform == OSPlatformType.Linux)
            {
                if (lineSplited[2].ToLower().Contains("active"))
                {
                    return ServiceStartupState.Active;
                }
                else if (lineSplited[2].ToLower().Contains("disabled"))
                {
                    return ServiceStartupState.Disabled;
                }
                else{
                    throw new SystemInformationException($"ServiceStartupState \"{lineSplited[2]}\" not recognized in linux system.");
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Platform not supported to get service startup state");
            }
        }

        private ServiceState GetServiceState(OSPlatformType osPlatformType, string[] lineSplited)
        {
            if (osPlatformType == OSPlatformType.Linux)
            {
                if (lineSplited[3].ToLower().Contains("running")) ///Service 
                {
                    return ServiceState.Running;
                }
                else if (lineSplited[3].ToLower().Contains("exited")) ///Service 
                {
                    return ServiceState.Stopped;
                }
                else{
                    throw new SystemInformationException($"ServiceStartupState \"{lineSplited[2]}\" not recognized in linux system.");
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Platform not supported to get service state");
            }
        }
    }
}
