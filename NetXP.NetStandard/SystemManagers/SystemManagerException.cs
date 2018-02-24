using System;
using System.Runtime.Serialization;

namespace NetXP.NetStandard.SystemManagers
{
    [Serializable]
    public class SystemManagerException : Exception
    {
        public SystemManagerException()
        {
        }

        public SystemManagerException(string message) : base(message)
        {
        }

        public SystemManagerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SystemManagerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}