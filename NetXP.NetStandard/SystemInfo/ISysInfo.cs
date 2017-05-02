using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.SystemInfo
{
    public interface ISysInfo
    {
        string MotherBoardSerialNumber();
        IDictionary<string, string> GetOSInfo();

        IDictionary<string, string> GetProcessorInfo();

        IDictionary<string, string> GetServiceInfo();

        IDictionary<string, string> GetBaseBoardInfo();

    }
}
