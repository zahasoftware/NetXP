using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetXP.NetStandard.Serialization.Implementations
{
    public class Serialize2Json : ISerializer
    {
        public T Deserialize<T>(byte[] bytesToDeserialize) where T : class, new()
        {
            if (bytesToDeserialize == null) return null;//return;
            using (MemoryStream ms = new MemoryStream(bytesToDeserialize))
            {
                var jsonSerializer = new DataContractJsonSerializer(typeof(T));
                ms.Position = 0;
                return jsonSerializer.ReadObject(ms) as T;
            }
        }

        public object Deserialize(Type type, byte[] bytesToDeserialize)
        {
            if (bytesToDeserialize == null) return null;//return;
            using (MemoryStream ms = new MemoryStream(bytesToDeserialize))
            {
                var jsonSerializer = new DataContractJsonSerializer(type);
                ms.Position = 0;
                return jsonSerializer.ReadObject(ms);
            }
        }

        public byte[] Serialize<T>(T objectToSerialize) where T : class
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                jsonSerializer.WriteObject(ms, objectToSerialize);
                ms.Position = 0;
                return ms.ToArray();
            }
        }
    }
}
