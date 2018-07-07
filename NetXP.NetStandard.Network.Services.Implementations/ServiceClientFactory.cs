using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Network.Services.Implementations
{
    public class ServiceClientFactory : IServiceClientFactory
    {
        private readonly IContainer container;

        public ServiceClientFactory(IContainer container)
        {
            this.container = container;
        }

        public IServiceClient Create(ServiceClientType serviceClientType)
        {
            switch (serviceClientType)
            {
                case ServiceClientType.SoapV11:
                    return container.Resolve<IServiceClient>(ServiceClientType.SoapV11.ToString());
                case ServiceClientType.SoapV12:
                    return container.Resolve<IServiceClient>(ServiceClientType.SoapV12.ToString());
                case ServiceClientType.Rest:
                    return container.Resolve<IServiceClient>(ServiceClientType.Rest.ToString());
                default:
                    return null;
            }
        }
    }
}
