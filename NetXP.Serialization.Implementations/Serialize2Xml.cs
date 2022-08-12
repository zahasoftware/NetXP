using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Serialization.Implementations
{
    public class Serialize2Xml : ISerializer
    {
        public T Deserialize<T>(byte[] bytesToDeserialize) where T : class
        {
            if (bytesToDeserialize == null) return null;

            using (MemoryStream ms = new MemoryStream(bytesToDeserialize))
            {
                var xmlSerializer = new DataContractSerializer(typeof(T));
                ms.Position = 0;
                return xmlSerializer.ReadObject(ms) as T;
            }
        }

        public object Deserialize(Type type, byte[] bytesToDeserialize)
        {
            if (bytesToDeserialize == null) return null;

            using (MemoryStream ms = new MemoryStream(bytesToDeserialize))
            {
                var xmlSerializer = new DataContractSerializer(type);
                ms.Position = 0;
                return xmlSerializer.ReadObject(ms);
            }
        }

        public byte[] Serialize<T>(T objectToSerialize) where T : class
        {
            var xmlSerializer = new DataContractSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                xmlSerializer.WriteObject(ms, objectToSerialize);
                ms.Position = 0;
                return ms.ToArray();
            }
        }
    }

}
