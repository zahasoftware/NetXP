using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetXP.NetStandard.Processes;
using NetXP.NetStandard.SystemInformation;

namespace NetXP.NetStandard.SystemManagers.Implementations
{
    public class ServiceManagerForWindows : IServiceManager
    {
        private readonly IIOTerminal terminal;
        private readonly ISystemInformation systemInformation;
        private readonly IServiceInformer serviceInformer;

        public ServiceManagerForWindows(
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

        public void Delete(string serviceName)
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

        public void Start(string serviceName)
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

        public void Stop(string serviceName)
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

        public void Uninstall(string serviceName)
        {
            var serviceInformation = this.serviceInformer.GetService(serviceName);
            if (serviceInformation.State == ServiceState.Running)
            {
                this.Stop(serviceName);
            }

            this.Delete(serviceName);
        }
    }
}