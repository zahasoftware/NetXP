using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NetXP.Exceptions;
using NetXP.Processes;

namespace NetXP.SystemInformation.Implementations
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

        public ServiceInformation GetService(string serviceName)
        {
            ServiceInformation si = null;
            var osInfo = systemInformation.GetOSInfo();
            //Window 
            if (osInfo.Platform == OSPlatformType.Windows)
            {
                var service = new ServiceController(serviceName);

                if (ExistsService(service))
                {
                    si = GetService(service);
                }
            }
            else if (osInfo.Platform == OSPlatformType.Linux)
            {
                si = GetServiceWithListUnits(serviceName);
                if (si == null)
                {
                    si = GetServiceWithListUnitsFile(serviceName);
                    if (si != null)
                    {
                        si.Description = "Not available";
                        si.State = GetServiceState(serviceName);
                    }
                }
            }
            return si;
        }

        private bool ExistsService(ServiceController service)
        {
            try
            {
                _ = service.Status;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private ServiceState GetServiceState(string serviceName)
        {
            serviceName = serviceName?.EndsWith(".service") == true ? serviceName : serviceName + ".service";

            ProcessOutput output = terminal.Execute(new ProcessInput
            {
                Command = $"systemctl status '{serviceName}'",
                ShellName = "/bin/bash",
            });

            if (output.StandardOutput.Count() > 1)
            {
                var outLine = output.StandardOutput.LastOrDefault(o => o.ToLower().Contains("failed")
                                                                    || o.ToLower().Contains("(failed)")
                                                                 );
                if (outLine != null)
                    return ServiceState.Failed;
                outLine = output.StandardOutput.LastOrDefault(o => o.ToLower().Contains("(running)"));
                if (outLine != null)
                    return ServiceState.Running;
                outLine = output.StandardOutput.LastOrDefault(o => o.ToLower().Contains("(exited)"));
                if (outLine != null)
                    return ServiceState.Stopped;

                return ServiceState.Unknown;
            }
            else
            {
                return ServiceState.Unknown;
            }


        }

        private ServiceInformation GetServiceWithListUnits(string serviceName)
        {
            List<ServiceInformation> servicesInformations = new List<ServiceInformation>();
            serviceName = serviceName?.EndsWith(".service") == true ? serviceName : serviceName + ".service";

            ProcessOutput output = terminal.Execute(new ProcessInput
            {
                Command = $"systemctl list-units --type=service '{serviceName}'",
                ShellName = "/bin/bash",
                Arguments = ""
            });

            foreach (var line in from x in output.StandardOutput.Skip(1)
                                 where x.Contains("service")
                                 select x
            )
            {
                var lineSplited = line.Replace("●", "") ///systemctl list failed service with "●", and It cannot parse normally
                        .Split(new char[] { ' ' }, 5, StringSplitOptions.RemoveEmptyEntries);
                if (lineSplited.Length != 5)
                {
                    throw new SystemInformationException("Service Output Failed, The systemctl Output has changed");
                }

                servicesInformations.Add(new ServiceInformation
                {
                    ServiceName = lineSplited[0],
                    Description = lineSplited[4],
                    State = GetServiceState(OSPlatformType.Linux, lineSplited[3], lineSplited[0]),
                    StartupState = GetServiceStartupState(OSPlatformType.Linux, lineSplited[2], lineSplited[0])
                });
            }


            return servicesInformations.Count > 0 ? servicesInformations.Single() : null;
        }

        private ServiceInformation GetServiceWithListUnitsFile(string serviceName)
        {
            List<ServiceInformation> servicesInformations = new List<ServiceInformation>();
            serviceName = serviceName?.EndsWith(".service") == true ? serviceName : serviceName + ".service";

            ProcessOutput output = terminal.Execute(new ProcessInput
            {
                Command = $"systemctl list-unit-files --type=service '{serviceName}'",
                ShellName = "/bin/bash",
                Arguments = ""
            });

            foreach (var line in from x in output.StandardOutput.Skip(1)
                                 where x.Contains("service")
                                 select x
            )
            {
                var lineSplited = line.Replace("●", "") ///systemctl list failed service with "●", and It cannot parse normally
                        .Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (lineSplited.Length != 2)
                {
                    throw new SystemInformationException("Service Output Failed, The systemctl file-list Output has changed");
                }

                servicesInformations.Add(new ServiceInformation
                {
                    ServiceName = lineSplited[0],
                    //Description = lineSplited[4],
                    //State = GetServiceState(OSPlatformType.Linux, lineSplited[1]),
                    StartupState = GetServiceStartupState(OSPlatformType.Linux, lineSplited[1], lineSplited[0])
                });
            }
            return servicesInformations.Count > 0 ? servicesInformations.Single() : null;
        }


        //TODO: Need to get all service, Include not installed service by error
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
                    ServiceInformation serviceInformation = GetService(service);
                    servicesInformations.Add(serviceInformation);
                }
            }
            else if (osInfo.Platform == OSPlatformType.Linux)
            {
                ProcessOutput output = terminal.Execute(new ProcessInput
                {
                    Command = "systemctl list-units --type=service | grep ''",
                    ShellName = "/bin/bash",
                    Arguments = ""
                });

                foreach (var line in output.StandardOutput.Skip(1))
                {
                    if (!line.Contains("service"))
                    {
                        continue;
                    }
                    var cleanedLine = line.Replace("●", ""); ///systemctl list failed service with "●", and It cannot parse normally
                    var lineSplited = cleanedLine.Split(new char[] { ' ' }, 5, StringSplitOptions.RemoveEmptyEntries);
                    if (lineSplited.Length != 5)
                    {
                        throw new SystemInformationException("Service Output Failed, The systemctl Output has changed");
                    }

                    servicesInformations.Add(new ServiceInformation
                    {
                        ServiceName = lineSplited[0],
                        Description = lineSplited[4],
                        State = GetServiceState(OSPlatformType.Linux, lineSplited[3], lineSplited[0]),
                        StartupState = GetServiceStartupState(OSPlatformType.Linux, lineSplited[2], lineSplited[0])
                    });
                }

            }

            return servicesInformations.Count > 0 ? servicesInformations : null;
        }

        private static ServiceInformation GetService(ServiceController service)
        {
            ServiceStartMode startType = ServiceStartMode.Disabled;
            try
            {
                startType = service.StartType;
            }
            catch (Exception)
            {
                startType = ServiceStartMode.Disabled;
            }

            var serviceInformation = new ServiceInformation()
            {
                Description = service.DisplayName,
                ServiceName = service.ServiceName,
                StartupState = startType == ServiceStartMode.Automatic || startType == ServiceStartMode.Boot ? ServiceStartupState.Active
                        /*else if*/: startType == ServiceStartMode.Disabled || startType == ServiceStartMode.Manual || startType == ServiceStartMode.System ? ServiceStartupState.Disabled
                        /*else*/ : throw new SystemInformationException("Cannot Determine Service Startup State"),
                State = service.Status == ServiceControllerStatus.Running ? ServiceState.Running : service.Status == ServiceControllerStatus.Stopped ? ServiceState.Stopped : ServiceState.Unknown,
            };
            return serviceInformation;
        }

        private ServiceStartupState GetServiceStartupState(OSPlatformType osPlatform, string startupState, string serviceName)
        {
            if (osPlatform == OSPlatformType.Linux)
            {
                if (startupState.ToLower().Contains("active")
                || startupState.ToLower().Contains("enabled"))
                {
                    return ServiceStartupState.Active;
                }
                else if (startupState.ToLower().Contains("disabled"))
                {
                    return ServiceStartupState.Disabled;
                }
                else if (startupState.ToLower().Contains("failed"))
                {
                    return ServiceStartupState.Failed;
                }
                else if (startupState.ToLower().Contains("activating"))
                {
                    return ServiceStartupState.Activating;
                }
                else
                {
                    throw new SystemInformationException($"ServiceStartupState \"{startupState}\" not recognized in linux system for service \"{serviceName}\".");
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Platform not supported to get service startup state");
            }
        }

        private ServiceState GetServiceState(OSPlatformType osPlatformType, string state, string serviceName)
        {
            if (osPlatformType == OSPlatformType.Linux)
            {
                if (state.ToLower().Contains("running")) ///Service 
                {
                    return ServiceState.Running;
                }
                else if (state.ToLower().Contains("exited")) ///Service 
                {
                    return ServiceState.Stopped;
                }
                else if (state.ToLower().Contains("")) ///Service 
                {
                    return ServiceState.Failed;
                }
                else
                {
                    throw new SystemInformationException($"ServiceStartupState \"{state}\" not recognized in linux system for service \"{serviceName}\"");
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Platform not supported to get service state");
            }
        }
    }
}