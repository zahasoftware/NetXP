using NetXP.NetStandard.SystemInformation;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.NetFramework.SystemInformation.Implementations
{
    public class SysInfo : ISystemInformation, IStorageInfo
    {
        public OSInfo GetOSInfo()
        {
            var aOperatingSystemInfo = ManagementClassToDictionary("Win32_OperatingSystem");
            var osInfo = new OSInfo();
            foreach (var values in aOperatingSystemInfo)
            {
                var osInfoType = osInfo.GetType();
                var property = osInfoType.GetProperty(values.Key);
                property?.SetValue(osInfo, values.Value);
            }
            return osInfo;
        }

        public IDictionary<string, string> GetProcessorInfo()
        {
            var aProcessInfo = ManagementClassToDictionary("Win32_Processor");
            //var aOSInfo = this.GetOSInfo();

            //var aUnique = from a in aProcessInfo
            //              where !aOSInfo.Any(o => o.Key == a.Key && o.Key == a.Key)
            //              select a;

            return aProcessInfo;
        }

        public IDictionary<string, string> GetServiceInfo()
        {
            var aServiceInfo = ManagementClassToDictionary("Win32_Service");
            return aServiceInfo;
        }

        public IDictionary<string, string> GetBaseBoardInfo()
        {
            return ManagementClassToDictionary("Win32_BaseBoard");
        }

        private static IDictionary<string, string> ManagementClassToDictionary(string sObject)
        {
            dynamic aOsInfo = new Dictionary<string, string>();
            try
            {
                ManagementClass osClass = new ManagementClass(sObject);
                foreach (ManagementObject queryObj in osClass.GetInstances())
                {
                    foreach (PropertyData prop in queryObj.Properties)
                    {
                        if (prop.Value != null)
                        {
                            aOsInfo[prop.Name] = prop.Value.ToString();
                        }
                    }
                }
            }
            catch (ManagementException)
            {
                throw;
            }
            return aOsInfo;
        }

        public string MotherBoardSerialNumber()
        {
            string mbInfo = String.Empty;

            //Get motherboard's serial number 
            ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select SerialNumber From Win32_BaseBoard");
            foreach (ManagementObject mo in mbs.Get())
            {
                mbInfo += mo["SerialNumber"].ToString();
            }
            return mbInfo;
            #region comment
            /*
            string mbInfo = String.Empty;
            ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
            scope.Connect();
            ManagementObject wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());

            foreach (PropertyData propData in wmiClass.Properties)
            {
                if (propData.Name == "SerialNumber")
                    mbInfo = String.Format("{0,-25}{1}", propData.Name, Convert.ToString(propData.Value));
            }
            */
            #endregion
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
