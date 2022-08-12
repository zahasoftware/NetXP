using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.Network.Services
{
    public interface IServiceClientFactory
    {
        IServiceClient Create(ServiceClientType serviceClientType);
    }
}
