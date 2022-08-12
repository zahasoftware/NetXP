using NetXP.Serialization;
using SoapHttpClient;
using SoapHttpClient.Enums;
using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NetXP.Network.Services.Implementations
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

            var rootXsiAttribute = xdoc.CreateAttribute("xmlns:xsi");
            rootXsiAttribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            root.Attributes.Append(rootXsiAttribute);

            foreach (var methodParam in methodParams)
            {
                var xparam = xdoc.CreateElement(methodParam.Name);
                root.AppendChild(xparam);

                var valueParam = methodParam.Value;
                var valueParamType = valueParam?.GetType();

                if ((valueParamType?.IsClass ?? false) && valueParamType != typeof(string))///Create a class structure
                {
                    foreach (var prop in valueParamType.GetProperties())
                    {
                        var xprop = xdoc.CreateElement(prop.Name);
                        xparam.AppendChild(xprop);
                        var propValue = prop.GetValue(valueParam);

                        if (prop.PropertyType.IsClass && !(propValue is string))
                        {
                            ///TODO: Do recursive
                        }
                        else if (prop.PropertyType.IsPrimitive || (propValue is string))
                        {
                            xprop.InnerText = (propValue is bool) ? propValue.ToString().ToLower()
                                            : xprop.InnerText = propValue.ToString();
                        }
                    }
                }
                else if (valueParamType == null)
                {
                    xparam.InnerText = "";
                    XmlAttribute nullAttribute = xdoc.CreateAttribute("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance");
                    nullAttribute.Value = "true";
                    xparam.Attributes.Append(nullAttribute);
                }
                else if (valueParamType.IsPrimitive || (valueParam is string))
                {
                    xparam.InnerText = valueParamType == typeof(bool)
                        ? valueParam.ToString().ToLower() : valueParam.ToString();
                }
            }

            var body = XElement.Parse(root.OuterXml);
            var bodies = new XElement[] { body };

            var httpClientHandler = new HttpClientHandler();

            //TODO: Need to change header to send SOAPAction with Soapclient in version 3.0.0 of SoapHttpClient nuget
            HttpClient httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(action))
            {
                httpClient.DefaultRequestHeaders.Add("SOAPAction", $"\"{action}\"");
            }

            string serializedResponse = null;
            var soapClient = new SoapHttpClient.SoapClient();
            {
                var result =
                  await soapClient.PostAsync(
                          endpoint: endPoint,
                          soapVersion: SoapVersion.Soap11,
                          bodies: bodies
                  );

                var serializedResponseEnvelop = await result.Content.ReadAsStringAsync();

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"SoapService, StatusCode: {(int)result.StatusCode} ({result.StatusCode})");
                }

                //Get Body Content 
                serializedResponse = XElement.Parse(XElement.Parse(serializedResponseEnvelop).FirstNode.ToString()).FirstNode.ToString();
            }

            if (typeof(T) != typeof(VoidDTO))
            {
                try
                {
                    serializedResponse = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + serializedResponse;
                    var serializedResponseInBytes = Encoding.UTF8.GetBytes(serializedResponse);
                    var deserializedResponse = xmlSerializerWithXmlSerializer.Deserialize<T>(serializedResponseInBytes);
                    return deserializedResponse;
                }
                catch (Exception exXMLSerializer)
                {
                    try
                    {
                        var serializedResponseInBytes = Encoding.UTF8.GetBytes(serializedResponse);
                        var deserializedResponse = xmlSerializerWithDataContractSerializer.Deserialize<T>(serializedResponseInBytes);
                        return deserializedResponse;
                    }
                    catch (Exception exDataContractSeralizer)
                    {
                        var ex = new SerializationException($"Error in SoapServiceClient, cannot deserializa with XMLSerializer " +
                            $"and DataContract, XMLSerializer message: {exXMLSerializer.Message}, DataContractSerializer message: " +
                            $"{exDataContractSeralizer.Message}]");
                        throw ex;
                    }
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
