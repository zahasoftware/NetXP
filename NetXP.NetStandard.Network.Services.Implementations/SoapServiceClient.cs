using NetXP.NetStandard.Serialization;
using SoapHttpClient;
using SoapHttpClient.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NetXP.NetStandard.Network.Services.Implementations
{
    public class SoapServiceClientV11 : IServiceClient
    {
        public static string InstanceNamespace = "";
        private readonly ISerializer xmlSerializerWithDataContractSerializer;
        private readonly ISerializer xmlSerializerWithXmlSerializer;

        public SoapServiceClientV11(ISerializerFactory serializerFactory)//, HttpClient customHttpClient = null)
        {
            this.xmlSerializerWithDataContractSerializer = serializerFactory.Resolve(SerializerType.Xml);
            this.xmlSerializerWithXmlSerializer = serializerFactory.Resolve(SerializerType.XmlSerializer);
        }

        public async Task Request(Uri endPoint, string methodName, string methodNamespace = null, string action = null, params MethodParam[] methodParams)
        {
            await this.Request<VoidDTO>(endPoint, methodName, methodNamespace, action, methodParams);
        }

        public async Task<T> Request<T>(
            Uri endPoint,
            string methodName,
            string methodNamespace = null,
            string action = null,
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

                if (valueParamType.IsClass && valueParamType != typeof(String))
                {
                    foreach (var prop in valueParamType.GetProperties())
                    {
                        var xprop = xdoc.CreateElement(prop.Name);
                        xparam.AppendChild(xprop);
                        var propValue = prop.GetValue(valueParam);

                        if (prop.PropertyType.IsClass && !(propValue is String))
                        {
                            ///TODO: Do recursive
                        }
                        else if (prop.PropertyType.IsPrimitive || (propValue is String))
                        {
                            if (propValue is bool)
                            {
                                xprop.InnerText = Convert.ToBoolean(propValue) == true ? "1" : "0";
                            }
                            else
                            {
                                xprop.InnerText = propValue.ToString();
                            }
                        }
                    }
                }
                else if (valueParamType.IsPrimitive || (valueParam is String))
                {
                    xparam.InnerText = valueParam.ToString();
                }
            }

            var body = XElement.Parse(root.OuterXml);
            var bodies = new XElement[] { body };

            var httpClientHandler = new HttpClientHandler();

            var httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(action))
            {
                httpClient.DefaultRequestHeaders.Add("SOAPAction", $"\"{action}\"");
            }

            string serializedResponse = null;
            using (var soapClient = new SoapClient(() => httpClient))
            {
                var result =
                  await soapClient.PostAsync(
                          endpoint: endPoint,
                          soapVersion: SoapVersion.Soap11,
                          bodies: bodies
                  );

                var serializedResponseEnvelop = await result.Content.ReadAsStringAsync();

                //Get Body Content 
                serializedResponse =
                    XElement.Parse(
                            XElement.Parse(serializedResponseEnvelop).FirstNode.ToString()
                        ).FirstNode.ToString();
            }

            if (typeof(T) != typeof(VoidDTO))
            {
                try
                {
                    serializedResponse += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
                    var serializedResponseInBytes = Encoding.UTF8.GetBytes(serializedResponse);
                    var deserializedResponse = xmlSerializerWithXmlSerializer.Deserialize<T>(serializedResponseInBytes);
                    return deserializedResponse;
                }
                catch (Exception ex)
                {
                    var serializedResponseInBytes = Encoding.UTF8.GetBytes(serializedResponse);
                    var deserializedResponse = xmlSerializerWithDataContractSerializer.Deserialize<T>(serializedResponseInBytes);
                    return deserializedResponse;
                }
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
