using System;
using System.Runtime.Serialization;

namespace NetXP.NetStandard.Network.Proxy.Implementations
{
    [Serializable]
    internal class ProxyConnectionException : Exception
    {
        public ProxyConnectionException()
        {
        }

        public ProxyConnectionException(string message) : base(message)
        {
        }

        public ProxyConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ProxyConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}