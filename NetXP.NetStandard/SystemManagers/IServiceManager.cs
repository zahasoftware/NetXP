using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.SystemManagers
{
    public interface IServiceManager
    {
        void Delete(string serviceName);
        void Stop(string serviceName);
        void Start(string serviceName);
        void Create(string serviceName, string binPath, ServiceCreateOptions serviceCreateOptions = null);
        void Uninstall(string serviceName);
    }
}
