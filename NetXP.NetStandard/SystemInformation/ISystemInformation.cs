using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.SystemInformation
{
    public interface ISystemInformation
    {
        string MotherBoardSerialNumber();
        OSInfo GetOSInfo();
    }
}
