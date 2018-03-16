using NetXP.NetStandard.SystemInformation;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.SystemManagers
{
    public interface IServiceManagerFactory
    {
        IServiceManager Create(OSPlatformType osPlatformType);
    }
}
