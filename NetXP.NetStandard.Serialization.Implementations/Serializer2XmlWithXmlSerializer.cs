using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetXP.NetStandard.Serialization.Implementations
{
    public class Serializer2XmlWithXmlSerializer : ISerializer
    {
        public T Deserialize<T>(byte[] bytesToDeserialize) where T : class
        {
            if (bytesToDeserialize == null) return null;

            using (MemoryStream ms = new MemoryStream(bytesToDeserialize))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                ms.Position = 0;
                return serializer.Deserialize(ms) as T;
            }
        }

        public object Deserialize(Type type, byte[] bytesToDeserialize)
        {
            if (bytesToDeserialize == null) return null;

            using (MemoryStream ms = new MemoryStream(bytesToDeserialize))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(type);
                ms.Position = 0;
                return xmlSerializer.Deserialize(ms);
            }
        }

        public byte[] Serialize<T>(T objectToSerialize) where T : class
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, objectToSerialize);
                ms.Position = 0;
                return ms.ToArray();
            }
        }
    }

}
