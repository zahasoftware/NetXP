using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.Services
{
    public interface IServiceClient : IDisposable
    {
        Task<T> Request<T>(
            Uri endPoint,
            string method,
            string @namespace = null,
            string action = null,
            params MethodParam[] @params
            ) where T : class;

        Task Request(
           Uri endPoint,
            string method,
            string @namespace = null,
            string action = null,
            params MethodParam[] @params
           );
    }
}
