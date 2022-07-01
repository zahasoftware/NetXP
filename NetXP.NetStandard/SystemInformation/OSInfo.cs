using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using NetXP.SystemInformation.Implementations;

namespace NetXP.SystemInformation
{
    public class OSInfo
    {
        public string Description { get; set; }
        public Architecture Architecture { get; set; }
        public string FrameworkDescription { get; set; }
        public OperatingSystem Version { get; set; }
        public OSPlatformType Platform { get; set; }
        public string Name { get; internal set; }
    }
}
