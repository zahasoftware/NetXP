using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.SystemInformation
{
    public interface ISystemInfo
    {
        string MotherBoardSerialNumber();
        OSInfo GetOSInfo();

        IDictionary<string, string> GetProcessorInfo();

        IDictionary<string, string> GetServiceInfo();

        IDictionary<string, string> GetBaseBoardInfo();


    }
}
