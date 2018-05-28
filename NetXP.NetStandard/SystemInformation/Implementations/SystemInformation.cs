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
            var storageInfoesToReturn = new List<StorageInfo>();
            var storageInfoes = DriveInfo.GetDrives().Where(o => o.IsReady);

            foreach (var o in storageInfoes)
            {
                bool @continue = true;
                while(@continue)
                try
                {
                    if (!Enum.TryParse(o.DriveType.ToString(), out NetStandard.SystemInformation.DriveType driveType))
                    {
                        driveType = DriveType.Unknown;
                    }

                        var storageInfo = new StorageInfo();
                        storageInfo.VolumeLabel = o.VolumeLabel;
                        storageInfo.AvailableFreeSpace = o.AvailableFreeSpace;
                        storageInfo.DriveFormat = o.DriveFormat;
                        storageInfo.TotalSize = o.TotalSize;
                        storageInfo.DriveType = driveType;
                        storageInfo.IsReady = o.IsReady;
                        storageInfo.Name = o.Name;
                        storageInfo.RootDirectory = o.RootDirectory;
                        storageInfo.TotalFreeSpace = o.TotalFreeSpace;

                        storageInfoesToReturn.Add(storageInfo);
                        @continue = false;
                    }
                    catch (IOException ioe)
                    {
                        if (ioe.HResult == 19)//No such device, when It is loading information yet
                        {
                            Task.Delay(500);
                        }
                        else if (ioe.HResult == 112)
                        {
                            Task.Delay(500);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
            }

            return storageInfoesToReturn.ToList();
        }
    }
}
