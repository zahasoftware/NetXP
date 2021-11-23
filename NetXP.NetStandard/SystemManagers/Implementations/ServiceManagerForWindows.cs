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

        public void Create(string serviceName, string binPath, ServiceCreateOptions serviceCreateOptions = null)
        {
            string createServiceString = $"/c sc create \"{serviceName}\" binPath= \"{binPath}\" ";

            if (!string.IsNullOrEmpty(serviceCreateOptions?.DisplayName?.Trim()))
            {
                createServiceString = $"{createServiceString} DisplayName= \"{serviceCreateOptions.DisplayName}\"";
            }

            var serviceMode = "delayed-auto";
            createServiceString = $"{createServiceString} start= \"{serviceMode}\"";

            var output = this.terminal.Execute(new ProcessInput
            {
                ShellName = "cmd",
                Arguments = createServiceString
            });

            if (output.ExitCode != 0)
            {
                throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardOutput));
            }

            if (serviceCreateOptions?.Restart == true)
            {
                var restartTimeout = "";

                for (int i = 0; i < 3; i++)
                {
                    restartTimeout += $"restart/{serviceCreateOptions.RestartSeconds * 1000}/";
                }
                restartTimeout = restartTimeout.Remove(restartTimeout.Length - 1, 1);

                string serviceRestart = $"/c sc failure \"{serviceName}\" reset= 60 actions= \"{restartTimeout}\" ";

                output = this.terminal.Execute(new ProcessInput
                {
                    ShellName = "cmd",
                    Arguments = serviceRestart
                });

                if (output.ExitCode != 0)
                {
                    throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardOutput));
                }
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
            this.Delete(serviceName);
            if (serviceInformation.State == ServiceState.Running)
            {
                this.Stop(serviceName);
            }
        }
    }
}