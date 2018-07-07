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

        public Task<T> Request<T>(Uri endPoint, string methodName, string methodNamespace = null, params MethodParam[] methodParams) where T : class
        {
            throw new NotImplementedException();
        }

        public Task Request(Uri endPoint, string methodName, string methodNamespace = null, params MethodParam[] methodParams)
        {
            throw new NotImplementedException();
        }
    }
}
