using NetXP.NetStandard.Processes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NetXP.NetStandard.SystemInformation.Implementations
{
    public class SysInfo : ISystemInformation, IStorageInfo
    {
        private readonly IIOTerminal ioTerminal;
        private string motherBoardSerialNumber;
        private string _OSPrettyName;

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
                           RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatformType.OSX : OSPlatformType.Unknown,
                Name = this.OSPrettyName(),
            };
            return osInfo;
        }

        private string OSPrettyName()
        {

            if (!string.IsNullOrEmpty(_OSPrettyName?.Trim()))
            {
                return _OSPrettyName;
            }

            string mbInfo = string.Empty;

            ProcessOutput result = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return RuntimeInformation.OSDescription;
            }
            //Raspberry, Linux (Fedora Tested, Debian)
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Act
                result = ioTerminal.Execute(new ProcessInput
                {
                    Command = @"cat /etc/os-release | grep PRETTY_NAME | cut -d ""="" -f2 | tail -c +2 | head -c -2",
                    ShellName = "/bin/bash",
                    Arguments = ""
                });

                result.StandardOutput = result.StandardOutput.Where(o => o?.Trim()?.Equals("") != true).ToArray();
                // Assert
            }

            string osPretyName = string.Join("", result?.StandardOutput)?.Trim();
            if (!string.IsNullOrEmpty(osPretyName))
            {
                this._OSPrettyName = osPretyName;
                return osPretyName;
            }
            else
            {
                return null;
            }

        }




        public string MotherBoardSerialNumber()
        {
            if (!string.IsNullOrEmpty(this.motherBoardSerialNumber))
            {
                return this.motherBoardSerialNumber;
            }

            string mbInfo = String.Empty;

            ProcessOutput result = null;
            string serial = "";
            int attempts = 0;

            do
            {

                if (GetOSInfo().Platform == SystemInformation.OSPlatformType.Windows)
                {
                    // Act
                    result = ioTerminal.Execute(new ProcessInput
                    {
                        ShellName = "cmd",
                        Arguments = "/c wmic baseboard get serialnumber"
                    });
                    result.StandardOutput =
                        result.StandardOutput.Where(o => !string.IsNullOrEmpty(o?.Trim())).Skip(1).Take(1)
                        .Where(o => o?.Trim()?.Equals("") != true).ToArray();
                    // Assert
                }
                //Raspberry
                else if (GetOSInfo().Platform == SystemInformation.OSPlatformType.Linux
                      && GetOSInfo().Architecture == Architecture.Arm || GetOSInfo().Architecture == Architecture.Arm64)
                {
                    // Act
                    result = ioTerminal.Execute(new ProcessInput
                    {
                        Command = "cat /proc/cpuinfo | grep Serial | awk ' {print $3}'",
                        ShellName = "/bin/bash",
                        Arguments = ""
                    });
                    result.StandardOutput =
                        result.StandardOutput
                        .Where(o => o?.Trim()?.Equals("") != true).ToArray();
                    // Assert
                }
                //Linux (Fedora Tested)
                else if (GetOSInfo().Platform == SystemInformation.OSPlatformType.Linux)
                {
                    // Act
                    result = ioTerminal.Execute(new ProcessInput
                    {
                        Command = "cat /sys/devices/virtual/dmi/id/board_serial",
                        ShellName = "/bin/bash",
                        Arguments = ""
                    });

                    result.StandardOutput =
                        result.StandardOutput
                        .Where(o => o?.Trim()?.Equals("") != true).ToArray();
                    // Assert
                }

                serial = string.Join("", result.StandardOutput);

                if (!string.IsNullOrEmpty(serial?.Trim()))
                {
                    this.motherBoardSerialNumber = serial;
                }
                else
                {
                    Task.Delay(1000).Wait();
                }
            } while (string.IsNullOrEmpty(serial) && attempts++ <= 3);

            return serial;
        }

        public ICollection<StorageInfo> GetStorageInfo()
        {
            var storageInfoesToReturn = new List<StorageInfo>();
            var storageInfoes = DriveInfo.GetDrives().Where(o => o.IsReady);

            foreach (var o in storageInfoes)
            {
                bool @continue = true;
                while (@continue)
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
                    catch (Exception)
                    {

                    }
            }

            return storageInfoesToReturn.ToList();
        }
    }
}
