using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using NetXP.NetStandard.SystemInformation.Implementations;

namespace NetXP.NetStandard.SystemInformation
{
    public class OSInfo
    {
        public string Description { get; set; }
        public Architecture Architecture { get; set; }
        public string FrameworkDescription { get; set; }
        public OperatingSystem Version { get; set; }
        public OSPlatformType Platform { get; set; }
    }
}
