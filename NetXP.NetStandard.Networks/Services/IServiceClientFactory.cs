using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Network.Services
{
    public interface IServiceClientFactory
    {
        IServiceClient Create(ServiceClientType serviceClientType);
    }
}
