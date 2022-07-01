using System;
using System.Runtime.Serialization;

namespace NetXP.Network.Proxy.Implementations
{
    [Serializable]
    internal class ProxyNotFoundException : Exception
    {
        public ProxyNotFoundException()
        {
        }

        public ProxyNotFoundException(string message) : base(message)
        {
        }

        public ProxyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ProxyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}