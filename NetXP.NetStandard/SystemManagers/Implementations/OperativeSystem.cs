using NetXP.NetStandard.Processes;
using NetXP.NetStandard.SystemInformation;
using NetXP.NetStandard.SystemInformation.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.SystemManagers.Implementations
{
    public class OperativeSystem : IOperativeSystem
    {
        private readonly IIOTerminal terminal;
        private readonly ISystemInformation systemInformation;

        public OperativeSystem(IIOTerminal terminal,
                               ISystemInformation systemInformation
            )
        {
            this.terminal = terminal;
            this.systemInformation = systemInformation;
        }

        public void Restart()
        {
            if (systemInformation.GetOSInfo().Platform == OSPlatformType.Windows)
            {
                terminal.Execute(new ProcessInput { Command = "shutdown -r -f -t 0", ShellName = "cmd" });
            }
            else if (systemInformation.GetOSInfo().Platform == OSPlatformType.Linux)
            {
                terminal.Execute(new ProcessInput { Command = "systemctl reboot", ShellName = "/bin/bash" });
            }
            else
            {
                throw new Exception("Cannot restart for this OS");
            }
        }

        public void Shutdown()
        {
            if (systemInformation.GetOSInfo().Platform == OSPlatformType.Windows)
            {
                terminal.Execute(new ProcessInput { Command = "shutdown -s -f -t 0", ShellName = "cmd" });
            }
            else if (systemInformation.GetOSInfo().Platform == OSPlatformType.Linux)
            {
                terminal.Execute(new ProcessInput { Command = "systemctl poweroff", ShellName = "/bin/bash" });
            }
            else
            {
                throw new Exception("Cannot poweroff for this OS");
            }
        }

    }
}
