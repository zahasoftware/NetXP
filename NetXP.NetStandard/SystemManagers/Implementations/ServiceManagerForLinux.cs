using NetXP.NetStandard.Processes;
using NetXP.NetStandard.SystemInformation;
using System;
using System.IO;

namespace NetXP.NetStandard.SystemManagers.Implementations
{
    public class ServiceManagerForLinux : IServiceManager
    {
        private readonly IIOTerminal terminal;
        private readonly ISystemInformation systemInformation;
        private readonly IServiceInformer serviceInformer;

        public ServiceManagerForLinux(
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
            try
            {
                var description = string.IsNullOrEmpty(serviceCreateOptions?.Description?.Trim()) ?
                    serviceName : serviceCreateOptions.Description;
                var after = string.IsNullOrEmpty(serviceCreateOptions?.After?.Trim()) ?
                    "network.target" : serviceCreateOptions.After;
                var wantedBy = string.IsNullOrEmpty(serviceCreateOptions?.WantedBy?.Trim()) ?
                    "multi-user.target" : serviceCreateOptions.WantedBy;
                var restartSeconds = serviceCreateOptions == null ? 5 : serviceCreateOptions.RestartSeconds;
                var restart = serviceCreateOptions?.Restart == null ? RestartConstants.Always : serviceCreateOptions.Restart;

                //https://access.redhat.com/documentation/en-us/red_hat_enterprise_linux/7/html/system_administrators_guide/sect-managing_services_with_systemd-unit_files
                var serviceFile = $"[Unit]{Environment.NewLine}";
                serviceFile += $"Description=\"{description}\"{Environment.NewLine}";
                //serviceFile += //$"After=\"{after}\"{Environment.NewLine}" +
                serviceFile += $"[Service]{Environment.NewLine}";
                serviceFile += $"ExecStart={binPath}{Environment.NewLine}";

                if (!string.IsNullOrEmpty(serviceCreateOptions?.WorkingDirectory))
                {
                    serviceFile += $"WorkingDirectory={serviceCreateOptions?.WorkingDirectory}{Environment.NewLine}";
                }
                serviceFile += $"Restart={restart.ToString()}{Environment.NewLine}";
                serviceFile += $"RestartSec={restartSeconds}{Environment.NewLine}";
                serviceFile += $"[Install]{Environment.NewLine}";
                serviceFile += $"WantedBy={wantedBy}{Environment.NewLine}";

                var outputFilePath = $"/etc/systemd/system/{serviceName}{(serviceName.Contains(".service") ? "" : ".service")}";

                using (var f = File.Open(outputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var w = new StreamWriter(f))
                {
                    w.Write(serviceFile);
                    w.Flush();
                }

                Enable(serviceName);
                ReloadDaemon();
            }
            catch (IOException ioe)
            {
                throw new SystemManagerException("Cannot make service", ioe);
            }
        }

        private void ReloadDaemon()
        {
            var output = terminal.Execute(new ProcessInput
            {
                ShellName = "/bin/bash",
                Command = $"systemctl daemon-reload"
            });

            if (output.ExitCode != 0)
            {
                throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardError));
            }
        }

        public void Delete(string serviceName)
        {
            var output = terminal.Execute(new ProcessInput
            {
                ShellName = "/bin/bash",
                Command = $"systemctl disable \"{serviceName}\""
            });

            var outputFilePath = $"/etc/systemd/system/{serviceName}{(serviceName.Contains(".service") ? "" : ".service")}";
            File.Delete(outputFilePath);

            if (output.ExitCode != 0 && output.ExitCode != 1)
            {
                throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardError));
            }
        }


        public void Enable(string serviceName)
        {
            var output = terminal.Execute(new ProcessInput
            {
                ShellName = "/bin/bash",
                Command = $"systemctl enable \"{serviceName}\""
            });

            if (output.ExitCode != 0)
            {
                throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardError));
            }
        }

        public void Start(string serviceName)
        {
            var output = terminal.Execute(new ProcessInput
            {
                ShellName = "/bin/bash",
                Command = $"systemctl start \"{serviceName}\""
            });

            if (output.ExitCode != 0)
            {
                throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardError));
            }

            var serviceInformation = serviceInformer.GetService(serviceName);
            if (serviceInformation.State == ServiceState.Failed)
            {
                throw new SystemManagerException("Service failed, See service state from systemctl for more information");
            }
        }

        public void Stop(string serviceName)
        {
            var serviceInformation = serviceInformer.GetService(serviceName);
            if (serviceInformation.State == ServiceState.Running)
            {
                var output = terminal.Execute(new ProcessInput
                {
                    ShellName = "/bin/bash",
                    Command = $"systemctl stop \"{serviceName}\""
                });

                if (output.ExitCode != 0)
                {
                    throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardError));
                }
            }
            else
            {
                throw new SystemManagerException("Service is not runnning.");
            }
        }

        public void Uninstall(string serviceName)
        {
            var serviceInformation = serviceInformer.GetService(serviceName);
            if (serviceInformation.State == ServiceState.Running)
            {
                Stop(serviceName);
            }
            Delete(serviceName);
        }
    }
}