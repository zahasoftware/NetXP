using NetXP.NetStandard.SystemInformation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NetXP.NetStandard.Processes;

namespace NetXP.NetStandard.SystemInformation.Implementations
{
    public class SysInfo : ISystemInformation, IStorageInfo
    {
        private readonly IIOTerminal ioTerminal;

        public SysInfo(IIOTerminal ioTerminal)
        {
            this.ioTerminal = ioTerminal;
        }

        public OSInfo GetOSInfo()
        {
            var osInfo = new OSInfo
            {
                Description = RuntimeInformation.OSDescription,
                Architecture = RuntimeInformation.OSArchitecture,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                Version = System.Environment.OSVersion,
                Platform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatformType.Linux :
                           RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatformType.Windows :
                           RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatformType.OSX :
                           OSPlatformType.Unknown,
            };
            return osInfo;
        }

        //public IDictionary<string, string> GetProcessorInfo()
        //{
        //    //var aProcessInfo = ManagementClassToDictionary("Win32_Processor");
        //    ////var aOSInfo = this.GetOSInfo();

        //    ////var aUnique = from a in aProcessInfo
        //    ////              where !aOSInfo.Any(o => o.Key == a.Key && o.Key == a.Key)
        //    ////              select a;

        //    //return aProcessInfo;
        //}

        public string MotherBoardSerialNumber()
        {
            string mbInfo = String.Empty;

            // Arrange
            ProcessOutput result = null;

            if (GetOSInfo().Platform == SystemInformation.OSPlatformType.Windows)
            {
                // Act
                result = this.ioTerminal.Execute(new ProcessInput
                {
                    ShellName = "cmd",
                    MaxOfSecondToWaitCommand = 5,
                    Arguments = "/c wmic baseboard get serialnumber"
                });
                result.StandardOutput =
                    result.StandardOutput.Where(o => !string.IsNullOrEmpty(o?.Trim())).Skip(1).Take(1)
                    .Where(o => o?.Trim()?.Equals("") != true).ToArray();
                // Assert
            }
            else if (GetOSInfo().Platform == SystemInformation.OSPlatformType.Linux)
            {
                // Act
                result = this.ioTerminal.Execute(new ProcessInput
                {
                    Command = "cat /sys/devices/virtual/dmi/id/board_serial",
                    ShellName = "/bin/bash",
                    MaxOfSecondToWaitCommand = 5,
                    Arguments = ""
                });
                result.StandardOutput =
                    result.StandardOutput
                    .Where(o => o?.Trim()?.Equals("") != true).ToArray();
                // Assert
            }

            return string.Join(",", result.StandardOutput);

        }

        public ICollection<StorageInfo> GetStorageInfo()
        {
            var storageInfoes = DriveInfo.GetDrives().Where(o => o.IsReady).Select(o =>
                                    new StorageInfo
                                    {
                                        VolumeLabel = o.VolumeLabel,
                                        AvailableFreeSpace = o.AvailableFreeSpace,
                                        DriveFormat = o.DriveFormat,
                                        TotalSize = o.TotalSize,
                                        DriveType = (NetStandard.SystemInformation.DriveType)Enum.Parse(typeof(NetStandard.SystemInformation.DriveType), o.DriveType.ToString()),
                                        IsReady = o.IsReady,
                                        Name = o.Name,
                                        RootDirectory = o.RootDirectory,
                                        TotalFreeSpace = o.TotalFreeSpace
                                    }
                                );
            return storageInfoes.ToList();
        }
    }
}
