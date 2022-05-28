using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Network.Services.Implementations
{
    public static class NetworkServicesCompositionRoot
    {
        public static void RegisterNetworkServices(this IRegister r)
        {
            r.Register<IServiceClient, SoapServiceClientV11>(ServiceClientType.SoapV11.ToString(), DILifeTime.Trasient);
            r.Register<IServiceClient, SoapServiceClientV11>(ServiceClientType.SoapV12.ToString(), DILifeTime.Trasient);
            r.Register<IServiceClient, RestClient>(ServiceClientType.Rest.ToString(), DILifeTime.Trasient);
            r.Register<IServiceClient, SoapServiceClientV11>(DILifeTime.Trasient);

            r.Register<IServiceClientFactory, ServiceClientFactory>();
        }

    }
}
