using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.SystemInformation
{
    public interface IServiceInformer
    {
        List<ServiceInformation> GetServices();
        ServiceInformation GetService(string serviceName);
    }
}
