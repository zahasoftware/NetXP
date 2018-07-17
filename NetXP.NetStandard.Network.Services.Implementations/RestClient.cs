using System;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.Services.Implementations
{
    public class RestClient : IServiceClient
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<T> Request<T>(Uri endPoint, string method, string @namespace = null, string action = null, params MethodParam[] @params) where T : class
        {
            throw new NotImplementedException();
        }

        public Task Request(Uri endPoint, string method, string @namespace = null, string action = null, params MethodParam[] @params)
        {
            throw new NotImplementedException();
        }
    }
}
