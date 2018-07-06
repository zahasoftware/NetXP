using NetXP.NetStandard.Serialization;
using SoapHttpClient;
using SoapHttpClient.Enums;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NetXP.NetStandard.Network.Services.Implementations
{
    public class SoapClient_1 : IServiceClient
    {
        public static string InstanceNamespace = "";
        private readonly ISerializer xmlSerializer;

        public SoapClient_1(ISerializerFactory serializerFactory)
        {
            this.xmlSerializer = serializerFactory.Resolve(SerializerType.Xml);
        }

        public async Task<T> Request<T>(
            Uri endPoint,
            string methodName,
            string methodNamespace = null,
            params MethodParam[] methodParams
            ) where T : class
        {
            XNamespace ns = XNamespace.Get(methodNamespace != null ? methodNamespace : "");

            var root = new XElement(ns + methodName);
            foreach (var methodParam in methodParams)
            {
                var xparam = new XElement(methodParam.Name);
                root.Add(xparam);

                var valueParam = methodParam.Value;
                var valueParamType = valueParam.GetType();
                if (valueParamType.IsClass)
                {
                    foreach (var prop in valueParamType.GetProperties())
                    {
                        var xprop = new XElement(prop.Name);
                        xparam.Add(xprop);
                        if (prop.PropertyType.IsClass)
                        {
                            ///TODO: Do recursive
                        }
                        else if (prop.PropertyType.IsPrimitive)
                        {
                            xprop.Value = prop.GetValue(valueParam).ToString();
                        }
                    }
                }
                else if (valueParamType.IsPrimitive)
                {
                    xparam.Value = valueParam.ToString();
                }
            }

            var body = new XElement[] { root };

            string serializedResponse = null;
            using (var soapClient = new SoapClient())
            {
                var result =
                  await soapClient.PostAsync(
                          endpoint: endPoint,
                          soapVersion: SoapVersion.Soap11,
                          bodies: body);


                var serializedResponseEnvelop = await result.Content.ReadAsStringAsync();

                //Get Body Content 
                serializedResponse =
                    XElement.Parse(
                            XElement.Parse(serializedResponseEnvelop).FirstNode.ToString()
                        ).FirstNode.ToString();
            }

            var serializedResponseInBytes = Encoding.UTF8.GetBytes(serializedResponse);
            var deserializedResponse = xmlSerializer.Deserialize<T>(serializedResponseInBytes);
            return deserializedResponse;
        }

        public void Dispose()
        {
        }
    }
}
