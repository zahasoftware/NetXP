using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.SystemInformation
{
    public interface IServiceInformer
    {
        List<ServiceInformation> GetServices();
        ServiceInformation GetService(string serviceName);
    }
}
