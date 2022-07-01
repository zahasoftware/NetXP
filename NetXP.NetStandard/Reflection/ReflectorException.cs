using System;
using System.Runtime.Serialization;

namespace NetXP.Reflection
{
    [Serializable]
    internal class ReflectorException : Exception
    {
        public ReflectorException()
        {
        }

        public ReflectorException(string message) : base(message)
        {
        }

        public ReflectorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReflectorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}