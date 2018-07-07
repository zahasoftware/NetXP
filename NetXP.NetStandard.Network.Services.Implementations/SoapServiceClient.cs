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
    public class SoapServiceClientV11 : IServiceClient
    {
        public static string InstanceNamespace = "";
        private readonly ISerializer xmlSerializer;

        public SoapServiceClientV11(ISerializerFactory serializerFactory)
        {
            this.xmlSerializer = serializerFactory.Resolve(SerializerType.Xml);
        }

        public async Task Request(Uri endPoint, string methodName, string methodNamespace = null, params MethodParam[] methodParams)
        {
            await this.Request<VoidDTO>(endPoint, methodName, methodNamespace, methodParams);
        }

        public async Task<T> Request<T>(
            Uri endPoint,
            string methodName,
            string methodNamespace = null,
            params MethodParam[] methodParams
            ) where T : class
        {
            var xdoc = new XmlDocument();
            var root = xdoc.CreateElement(methodName);

            ///Adding xmlns namespace attribute
            var rootXmlnsAttribute = xdoc.CreateAttribute("xmlns");
            rootXmlnsAttribute.Value = methodNamespace ?? "";
            root.Attributes.Append(rootXmlnsAttribute);

            foreach (var methodParam in methodParams)
            {
                var xparam = xdoc.CreateElement(methodParam.Name);
                root.AppendChild(xparam);

                var valueParam = methodParam.Value;
                var valueParamType = valueParam.GetType();

                if (valueParamType.IsClass)
                {
                    foreach (var prop in valueParamType.GetProperties())
                    {
                        var xprop = xdoc.CreateElement(prop.Name);
                        xparam.AppendChild(xprop);
                        if (prop.PropertyType.IsClass && !(prop.GetValue(valueParam) is String))
                        {
                            ///TODO: Do recursive
                        }
                        else if (prop.PropertyType.IsPrimitive || (prop.GetValue(valueParam) is String))
                        {
                            if (prop.GetValue(valueParam) is bool)
                            {
                                xprop.InnerText = Convert.ToBoolean(prop.GetValue(valueParam)) == true ? "1" : "0";
                            }
                            else
                            {
                                xprop.InnerText = prop.GetValue(valueParam).ToString();
                            }
                        }
                    }
                }
                else if (valueParamType.IsPrimitive)
                {
                    xparam.InnerText = valueParam.ToString();
                }
            }

            var body = XElement.Parse(root.OuterXml);
            var bodies = new XElement[] { body };

            string serializedResponse = null;
            using (var soapClient = new SoapClient())
            {
                var result =
                  await soapClient.PostAsync(
                          endpoint: endPoint,
                          soapVersion: SoapVersion.Soap11,
                          bodies: bodies);


                var serializedResponseEnvelop = await result.Content.ReadAsStringAsync();

                //Get Body Content 
                serializedResponse =
                    XElement.Parse(
                            XElement.Parse(serializedResponseEnvelop).FirstNode.ToString()
                        ).FirstNode.ToString();
            }

            if (typeof(T) != typeof(VoidDTO))
            {
                var serializedResponseInBytes = Encoding.UTF8.GetBytes(serializedResponse);
                var deserializedResponse = xmlSerializer.Deserialize<T>(serializedResponseInBytes);
                return deserializedResponse;
            }
            else
            {
                return null;
            }
        }

        public void Dispose()
        {
        }


    }
}
