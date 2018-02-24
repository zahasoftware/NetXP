using NetXP.NetStandard.Processes;
using NetXP.NetStandard.SystemInformation;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.SystemManagers.Implementations
{
    public class ServiceManager : IServiceManager
    {
        private readonly IIOTerminal terminal;
        private readonly ISystemInformation systemInformation;
        private readonly IServiceInformer serviceInformer;

        public ServiceManager(
            IIOTerminal terminal,
            ISystemInformation systemInformation,
            IServiceInformer serviceInformer
            )
        {
            this.terminal = terminal;
            this.systemInformation = systemInformation;
            this.serviceInformer = serviceInformer;
        }

        public void Create(string serviceName, string binPath, ServiceCreateOptions serviceCreateOptions)
        {
            if (systemInformation.GetOSInfo().Platform == SystemInformation.Implementations.OSPlatformType.Windows)
            {
                string createServiceString = $"/c sc create \"{serviceName}\" binPath= \"{binPath}\" ";

                if (!string.IsNullOrEmpty(serviceCreateOptions?.DisplayName?.Trim()))
                {
                    createServiceString = $"{createServiceString} DisplayName= \"{serviceCreateOptions.DisplayName}\"";
                }

                if (serviceCreateOptions != null)
                {
                    var serviceMode = serviceCreateOptions.ServiceStartupState == ServiceStartupState.Active ? "delayed-auto" :
                                      serviceCreateOptions.ServiceStartupState == ServiceStartupState.Disabled ? "demand" : "delayed-auto";
                    createServiceString = $"{createServiceString} start= \"{serviceMode}\"";
                }
                else
                {
                    createServiceString = $"{createServiceString} start= delayed-auto";
                }

                var output = this.terminal.Execute(new ProcessInput
                {
                    ShellName = "cmd",
                    Arguments = createServiceString
                });

                if (output.ExitCode != 0)
                {
                    throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardOutput));
                }
            }
            else if (systemInformation.GetOSInfo().Platform == SystemInformation.Implementations.OSPlatformType.Linux)
            {

            }
        }

        public void Delete(string serviceName)
        {
            if (systemInformation.GetOSInfo().Platform == SystemInformation.Implementations.OSPlatformType.Windows)
            {
                var output = this.terminal.Execute(new ProcessInput
                {
                    ShellName = "cmd",
                    Arguments = $"/c sc delete \"{serviceName}\""
                });

                if (output.ExitCode != 0)
                {
                    throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardOutput));
                }
            }
            else if (systemInformation.GetOSInfo().Platform == SystemInformation.Implementations.OSPlatformType.Linux)
            {

            }
        }

        public void Start(string serviceName)
        {
            if (systemInformation.GetOSInfo().Platform == SystemInformation.Implementations.OSPlatformType.Windows)
            {
                var output = this.terminal.Execute(new ProcessInput
                {
                    ShellName = "cmd",
                    Arguments = $"/c sc start \"{serviceName}\""
                });

                if (output.ExitCode != 0)
                {
                    throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardOutput));
                }
            }
            else if (systemInformation.GetOSInfo().Platform == SystemInformation.Implementations.OSPlatformType.Linux)
            {

            }
        }

        public void Stop(string serviceName)
        {
            if (systemInformation.GetOSInfo().Platform == SystemInformation.Implementations.OSPlatformType.Windows)
            {
                var output = this.terminal.Execute(new ProcessInput
                {
                    ShellName = "cmd",
                    Arguments = $"/c sc stop \"{serviceName}\""
                });

                if (output.ExitCode != 0)
                {
                    throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardOutput));
                }
            }
            else if (systemInformation.GetOSInfo().Platform == SystemInformation.Implementations.OSPlatformType.Linux)
            {

            }
        }

        public void Uninstall(string serviceName)
        {
            if (systemInformation.GetOSInfo().Platform == SystemInformation.Implementations.OSPlatformType.Windows)
            {
                var serviceInformation = this.serviceInformer.GetService(serviceName);
                if (serviceInformation.State == ServiceState.Running)
                {
                    this.Stop(serviceName);
                }

                this.Delete(serviceName);
            }
            else if (systemInformation.GetOSInfo().Platform == SystemInformation.Implementations.OSPlatformType.Linux)
            {

            }
        }
    }
}
