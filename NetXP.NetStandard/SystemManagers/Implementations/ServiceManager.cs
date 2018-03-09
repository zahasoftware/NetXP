using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetXP.NetStandard.Processes;
using NetXP.NetStandard.SystemInformation;

namespace NetXP.NetStandard.SystemManagers.Implementations {
    public class ServiceManager : IServiceManager {
        private readonly IIOTerminal terminal;
        private readonly ISystemInformation systemInformation;
        private readonly IServiceInformer serviceInformer;

        public ServiceManager (
            IIOTerminal terminal,
            ISystemInformation systemInformation,
            IServiceInformer serviceInformer
        ) {
            this.terminal = terminal;
            this.systemInformation = systemInformation;
            this.serviceInformer = serviceInformer;
        }

        public void Create (string serviceName, string binPath, ServiceCreateOptions serviceCreateOptions) {
            if (systemInformation.GetOSInfo ().Platform == SystemInformation.Implementations.OSPlatformType.Windows) {
                string createServiceString = $"/c sc create \"{serviceName}\" binPath= \"{binPath}\" ";

                if (!string.IsNullOrEmpty (serviceCreateOptions?.DisplayName?.Trim ())) {
                    createServiceString = $"{createServiceString} DisplayName= \"{serviceCreateOptions.DisplayName}\"";
                }

                if (serviceCreateOptions != null) {
                    var serviceMode = serviceCreateOptions.ServiceStartupState == ServiceStartupState.Active ? "delayed-auto" :
                        serviceCreateOptions.ServiceStartupState == ServiceStartupState.Disabled ? "demand" : "delayed-auto";
                    createServiceString = $"{createServiceString} start= \"{serviceMode}\"";
                } else {
                    createServiceString = $"{createServiceString} start= delayed-auto";
                }

                var output = this.terminal.Execute (new ProcessInput {
                    ShellName = "cmd",
                        Arguments = createServiceString
                });

                if (output.ExitCode != 0) {
                    throw new SystemManagerException (string.Join (Environment.NewLine, output.StandardOutput));
                }
            } else if (systemInformation.GetOSInfo ().Platform == SystemInformation.Implementations.OSPlatformType.Linux) {
                try {
                    var description = string.IsNullOrEmpty (serviceCreateOptions?.Description?.Trim ()) ?
                        serviceName : serviceCreateOptions.Description;
                    var after = string.IsNullOrEmpty (serviceCreateOptions?.After?.Trim ()) ?
                        "network.target" : serviceCreateOptions.After;
                    var wantedBy = string.IsNullOrEmpty (serviceCreateOptions?.WantedBy?.Trim ()) ?
                        "default.target" : serviceCreateOptions.WantedBy;

                    //https://access.redhat.com/documentation/en-us/red_hat_enterprise_linux/7/html/system_administrators_guide/sect-managing_services_with_systemd-unit_files
                    var serviceFile =
                        $"[Unit]{Environment.NewLine}" +
                        $"Description=\"{description}\"{Environment.NewLine}" +
                        $"After=\"{after}\"{Environment.NewLine}" +
                        $"[Service]{Environment.NewLine}" +
                        $"ExecStart={binPath}{Environment.NewLine}" +
                        $"[Install]{Environment.NewLine}" +
                        $"WantedBy={wantedBy}{Environment.NewLine}";

                    var outputFilePath = $"/etc/systemd/system/{serviceName}{(serviceName.Contains(".service") ? "" : ".service")}";

                    using (var f = File.Open (outputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    using (var w = new StreamWriter (f)) {
                        w.Write (serviceFile);
                    }
                } catch (IOException ioe) {
                    throw new SystemManagerException ("Cannot make service", ioe);
                }
            }
        }

        public void Delete (string serviceName) {
            if (systemInformation.GetOSInfo ().Platform == SystemInformation.Implementations.OSPlatformType.Windows) {
                var output = this.terminal.Execute (new ProcessInput {
                    ShellName = "cmd",
                        Arguments = $"/c sc delete \"{serviceName}\""
                });

                if (output.ExitCode != 0) {
                    throw new SystemManagerException (string.Join (Environment.NewLine, output.StandardOutput));
                }
            } else if (systemInformation.GetOSInfo ().Platform == SystemInformation.Implementations.OSPlatformType.Linux) {
                try
                {
                    var output = this.terminal.Execute(new ProcessInput
                    {
                        ShellName = "/bin/bash",
                        Command = $"systemctl disable \"{serviceName}\""
                    });

                    if (output.ExitCode != 0)
                    {
                        throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardError));
                    }

                    var outputFilePath = $"/etc/systemd/system/{serviceName}{(serviceName.Contains(".service") ? "" : ".service")}";
                    File.Delete(outputFilePath);
                }
                catch (IOException ioe)
                {
                    throw new SystemManagerException("Cannot make service", ioe);
                }
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
                var output = this.terminal.Execute(new ProcessInput
                {
                    ShellName = "/bin/bash",
                    Command = $"systemctl start \"{serviceName}\""
                });

                if (output.ExitCode != 0)
                {
                    throw new SystemManagerException(string.Join(Environment.NewLine, output.StandardError));
                }

                var serviceInformation = this.serviceInformer.GetService(serviceName);
                if (serviceInformation.State == ServiceState.Failed)
                {
                    throw new SystemManagerException("Service failed, See service state from systemctl for more information");
                }
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
                var serviceInformation = this.serviceInformer.GetService(serviceName);
                if (serviceInformation.State == ServiceState.Running)
                {
                    var output = this.terminal.Execute(new ProcessInput
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