using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetXP.CompositionRoots;
using NetXP.DependencyInjection;
using NetXP.DependencyInjection.Implementations.StructureMaps;
using NetXP.NetCore;
using NetXP.Processes;
using StructureMap;
using System;
using System.Linq;

namespace NetXP.NetCoreUnitTest.Processes.Implementations
{
    [TestClass]
    public class IOTerminalTests
    {
        public DependencyInjection.IContainer container;

        [TestInitialize]
        public void Init()
        {
            Container smapContainer = new Container();

            container = new SMContainer(smapContainer);
            container.Configuration.Configure((IRegister cnf) =>
                   {
                       cnf.RegisterAllNetXP();
                   });
        }

        [TestMethod]
        public void NC_IOTerminal_Execute()
        {
            // Arrange
            var systemInformation = container.Resolve<NetStandard.SystemInformation.ISystemInformation>();
            ProcessOutput result = null;

            if (systemInformation.GetOSInfo().Platform == SystemInformation.OSPlatformType.Windows)
            {
                // Act
                IIOTerminal iOTerminal = this.CreateIOTerminal();
                result = iOTerminal.Execute(new ProcessInput
                {
                    Command = "Get-WmiObject Win32_BaseBoard | Format-Wide -Property SerialNumber",
                    ShellName = "powershell",
                    Arguments = "-OutputFormat Text -InputFormat Text -File -"
                });
                result.StandardOutput =
                    result.StandardOutput.Skip(1).SkipLast(1)
                    .Where(o => o?.Trim()?.Equals("") != true).ToArray();
                // Assert
            }
            else if (systemInformation.GetOSInfo().Platform == SystemInformation.OSPlatformType.Linux)
            {
                // Act
                IIOTerminal iOTerminal = this.CreateIOTerminal();
                result = iOTerminal.Execute(new ProcessInput
                {
                    Command = "/sys/devices/virtual/dmi/id/board_serial",
                    ShellName = "/bin/bash",
                    Arguments = ""
                });
                result.StandardOutput =
                    result.StandardOutput.Skip(1).SkipLast(1)
                    .Where(o => o?.Trim()?.Equals("") != true).ToArray();
                // Assert
            }

            Console.WriteLine(string.Join(",", result.StandardOutput));
            Assert.AreNotEqual(result.StandardOutput, 0);
        }

        private IIOTerminal CreateIOTerminal()
        {
            return container.Resolve<IIOTerminal>();
        }
    }
}
