using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.Services
{
    public interface IServiceClient : IDisposable
    {
        Task<T> Request<T>(
            Uri endPoint,
            string methodName,
            string methodNamespace = null,
            params MethodParam[] methodParams
            ) where T : class;

        Task Request(
           Uri endPoint,
           string methodName,
           string methodNamespace = null,
           params MethodParam[] methodParams
           );
    }
}
