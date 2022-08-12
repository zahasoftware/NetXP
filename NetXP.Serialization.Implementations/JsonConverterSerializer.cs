using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetXP.Serialization.Implementations
{
    public class JsonConverterSerializer : ISerializer
    {
        public T Deserialize<T>(byte[] bytesToDeserialize) where T : class
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytesToDeserialize));
        }

        public object Deserialize(Type type, byte[] bytesToDeserialize)
        {
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytesToDeserialize), type);
        }

        public byte[] Serialize<T>(T objectToSerialize) where T : class
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(objectToSerialize));
        }
    }
}
