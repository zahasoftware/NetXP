using NetXP.SystemInformation;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.SystemManagers
{
    public interface IServiceManagerFactory
    {
        IServiceManager Create(OSPlatformType osPlatformType);
    }
}
