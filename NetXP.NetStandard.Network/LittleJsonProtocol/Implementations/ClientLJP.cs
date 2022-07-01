using Microsoft.Extensions.Options;
using NetXP.Auditory;
using NetXP.Network.TCP;
using NetXP.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.LittleJsonProtocol.Implementations
{
    public class ClientLJP : IClientLJP, IDisposable
    {
        private readonly IReflector reflector;
        private readonly ILogger logger;
        private readonly List<Type> aPrimitiveTypes = new List<Type>{
            typeof(string)
            , typeof(int)   , typeof(int?)
            , typeof(float) , typeof(float?)
            , typeof(long)  , typeof(long?)
            , typeof(double), typeof(double?)
            , typeof(bool)  , typeof(bool?)
            /// .... etc...
        };

        private byte[] receiveBuffer = new byte[1024 * 1024];
        private readonly byte[] nullByteInArray = new byte[] { 0 };
        private readonly IDictionary<string, LJPDNSCache> ipsCachedDictionary = new Dictionary<string, LJPDNSCache>();
        private readonly IFactoryClientLJP factoryClientLJP;

        public ClientLJP
        (
              IClientConnectorFactory factoryConnectorFactory
            , IReflector reflector
            , ILogger logger
            , IFactoryClientLJP factoryClientLJP
            , IOptions<ClientLJPOptions> options
            )
        {
            this.logger = logger;
            this.reflector = reflector;
            this.factoryClientLJP = factoryClientLJP;
            this.ClientConnector = factoryConnectorFactory.Create();

            this.options = options.Value;
        }

        public void SendCall(LJPCall sendCallParameter)
        {
            if (sendCallParameter == null)
            {
                SendResponse(null);//No Data
            }
            else
            {
                var messageExtractor = factoryClientLJP.CreateMessageFactory(sendCallParameter.Version);
                string jsonFinal = messageExtractor.Parse(sendCallParameter);

                string message =
                      $"Version={sendCallParameter.Version}\n"
                    + $"Length={Encoding.UTF8.GetBytes(jsonFinal).Length}\n"
                    + $"Id={sendCallParameter.Id}\n"
                    + $"KeepAlive={sendCallParameter.KeepAlive}\n"
                    + $"NeedResponse={sendCallParameter.NeedResponse}\n"
                    + $"Interface={sendCallParameter.InterfaceName}\n"
                    + $"Method={sendCallParameter.MethodName}\n"
                    + "\n"
                    + $"{jsonFinal}";

#if DEBUG
                logger.Debug($"SendCall From [{ClientConnector.LocalEndPoint?.ToString() ?? ""}] To [{ClientConnector.RemoteEndPoint?.ToString() ?? ""}], Msg = {jsonFinal.Replace("\n", "[nl]")}");
#endif

                var aMessage = Encoding.UTF8.GetBytes(message);

                ClientConnector.Send(aMessage, 0, aMessage.Length);
            }
        }

        public void SendResponse(object oObject)
        {
            if (oObject == null)
            {
                oObject = new LJPExceptionDTO() { Message = "No data", IClientLJPExceptionType = (int)LJPExceptionType.NoData };
            }

            var type = oObject.GetType();
            //DataContractJsonSerializer oJsonSerializer = new(type);
            //MemoryStream oMS = new();
            //oJsonSerializer.WriteObject(oMS, oObject);//Deserialize the oObject into Memory Stream (oMS).
            //oMS.Position = 0;
            //string sJson = new StreamReader(oMS).ReadToEnd();//Read object deserialized.

            var json = JsonConvert.SerializeObject(oObject);

            int iLength = Encoding.UTF8.GetBytes(json).Length;

            string message = string.Format(
                  "Length={0}\n"
                + "Type={1}\n"
                + "\n{2}"
                , iLength
                , !oObject.GetType().GetTypeInfo().IsGenericType ? oObject.GetType().Name : oObject.GetType().FullName
                , json
            );

#if DEBUG
            logger.Debug($"SendReponse [Lenght={iLength}] From [{ClientConnector.LocalEndPoint?.ToString() ?? ""}] - To [{ClientConnector.RemoteEndPoint?.ToString() ?? ""}] {json.Replace("\n", "[nl]")}");
#endif
            var aMessage = Encoding.UTF8.GetBytes(message);

            ClientConnector.Send(aMessage, 0, aMessage.Length);
        }


        public LJPCallReceived ReceiveCall(params string[] servicesLayers)
        {
            byte[] dinamycBufferToAllMessage = null;
            try
            {
                LJPCallReceived oLJPCallResponse = new LJPCallReceived();

                //Receive Header 
                Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
                ClientConnector.Receive(receiveBuffer, 0, receiveBuffer.Length);
                var indexOfHeaderAndBodySeparator = ByteHelper.IndexOf(receiveBuffer
                                                        , 0
                                                        , new byte[] { Convert.ToByte('\n'), Convert.ToByte('\n') });
                if (indexOfHeaderAndBodySeparator == -1)
                {
                    throw new LJPException("Bad little json protocol, header body separator not found.") { LJPExceptionType = LJPExceptionType.BadProtocol };
                }

                #region Parsing Raw Data Header Of SendCall Message

                var headerBytes = new byte[indexOfHeaderAndBodySeparator];
                Array.Copy(receiveBuffer, headerBytes, indexOfHeaderAndBodySeparator);
                string headerUTF8 = Encoding.UTF8.GetString(headerBytes);
                var headerUTF8Splited = headerUTF8.Split('\n');

                //string message = Encoding.UTF8.GetString(this.aReceiveBuffer);
#if DEBUG
                logger.Debug($"Header=[{string.Join(", ", headerUTF8Splited) }]");
#endif

                var lengthLine = headerUTF8Splited.SingleOrDefault(o => o.Contains("Length="));
                var commandIdLine = headerUTF8Splited.SingleOrDefault(o => o.Contains("Id="));
                var stayAliveLine = headerUTF8Splited.SingleOrDefault(o => o.Contains("KeepAlive="));
                var needResponseLine = headerUTF8Splited.SingleOrDefault(o => o.Contains("NeedResponse="));
                var interfaceLine = headerUTF8Splited.SingleOrDefault(o => o.Contains("Interface="));
                var methodLine = headerUTF8Splited.SingleOrDefault(o => o.Contains("Method="));
                var versionLine = headerUTF8Splited.SingleOrDefault(o => o.Contains("Version="));
                var typeLine = headerUTF8Splited.SingleOrDefault(o => o.Contains("Type="));

                if (versionLine == null) { versionLine = "Version=0.0"; }

                //If is a exception
                if (typeLine != null && typeLine.Split(new char[] { '=' }, 2)[1] == nameof(LJPExceptionDTO))
                {
                    ConvertToLJPResponse(new List<Type>() { typeof(LJPExceptionDTO) }, receiveBuffer, true);
                }

                if (lengthLine == null
                    || commandIdLine == null
                    || stayAliveLine == null
                    || needResponseLine == null
                    || interfaceLine == null
                    || methodLine == null)
                {
                    throw new LJPException("Bad little json protocol header, all or some fields nulls.") { LJPExceptionType = LJPExceptionType.BadProtocol };
                }

                var rawLength = lengthLine.Split(new char[] { '=' }, 2)[1];
                var rawCommandId = commandIdLine.Split(new char[] { '=' }, 2)[1];
                var rawStayAlive = stayAliveLine.Split(new char[] { '=' }, 2)[1];
                var rawNeedResponse = needResponseLine.Split(new char[] { '=' }, 2)[1];
                var rawInterface = interfaceLine.Split(new char[] { '=' }, 2)[1];
                var rawMethod = methodLine.Split(new char[] { '=' }, 2)[1];
                var rawVersion = versionLine.Split(new char[] { '=' }, 2)[1];

                #endregion
             
                #region Converting header types to layer types

                if (!long.TryParse(rawCommandId, out long commandId))
                    oLJPCallResponse.Id = null;
                else
                    oLJPCallResponse.Id = commandId;

                oLJPCallResponse.iLength = int.Parse(rawLength);
                oLJPCallResponse.KeepAlive = bool.Parse(rawStayAlive);
                oLJPCallResponse.NeedResponse = bool.Parse(rawNeedResponse);
                oLJPCallResponse.Version = rawVersion;
                KeepAlive = oLJPCallResponse.KeepAlive;

                Type serviceInterface = null;
                Assembly ass = null;
                foreach (var serviceLayer in servicesLayers)
                {
                    if (Path.GetExtension(serviceLayer) == ".dll")
                    {
                        //ass = Assembly.LoadFrom(serviceLayer);
                        throw new NotImplementedException();
                    }
                    else
                    {
                        ass = Assembly.Load(new AssemblyName(serviceLayer));
                    }

                    serviceInterface = ass.GetTypes().SingleOrDefault(o => o.Name.Equals(rawInterface, StringComparison.OrdinalIgnoreCase));
                    if (serviceInterface != null)
                    {
                        var methodOfInterface = serviceInterface.GetTypeInfo().GetMethod(rawMethod);
                        if (methodOfInterface != null)
                        {
                            oLJPCallResponse.Method = methodOfInterface;
                            break;
                        }
                    }
                }

                oLJPCallResponse.Interface = serviceInterface ?? throw new LJPException($"Bad little json protocol, Interface or method not found in layers:\n\t{string.Join("\n\t", serviceInterface)}", LJPExceptionType.FormatException);

                #endregion

                #region Receiving the json object string part
                int indexOfEndOfBody = ByteHelper.IndexOf(receiveBuffer, indexOfHeaderAndBodySeparator + 2, new byte[] { Convert.ToByte('\0') });
                indexOfEndOfBody = indexOfEndOfBody == -1 ? receiveBuffer.Length : indexOfEndOfBody;

                var firstExtractOfBody = new byte[indexOfEndOfBody - (indexOfHeaderAndBodySeparator + 2)];
                if (firstExtractOfBody.Length > oLJPCallResponse.iLength)
                {
                    throw new LJPException($"Bad body length Had={firstExtractOfBody.Length}, Expected={oLJPCallResponse.iLength}.");
                }

                Buffer.BlockCopy(receiveBuffer, indexOfHeaderAndBodySeparator + 2, firstExtractOfBody, 0, firstExtractOfBody.Length);
                int receivedToNow = firstExtractOfBody.Length;

                dinamycBufferToAllMessage = new byte[oLJPCallResponse.iLength];

                Buffer.BlockCopy(firstExtractOfBody, 0, dinamycBufferToAllMessage, 0, receivedToNow);
                //Whether there is more fragment of the object
                while (receivedToNow < oLJPCallResponse.iLength)
                {
                    Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
                    ClientConnector.Receive(receiveBuffer, 0, receiveBuffer.Length);

                    int indexOfEnd = ByteHelper.IndexOf(receiveBuffer, 0, nullByteInArray);
                    indexOfEnd = indexOfEnd == -1 ? receiveBuffer.Length : indexOfEnd;

                    Buffer.BlockCopy(receiveBuffer, 0, dinamycBufferToAllMessage, receivedToNow, indexOfEnd);
                    receivedToNow += indexOfEnd;

                    if (indexOfEnd == 0 || indexOfEnd == -1)
                        break;
                }

                var sObject = Encoding.UTF8.GetString(dinamycBufferToAllMessage);
                #endregion

                //Converting message 
                var messageExtractor = factoryClientLJP.CreateMessageFactory(oLJPCallResponse.Version);
                messageExtractor.Parse(oLJPCallResponse, sObject);

                return oLJPCallResponse;
            }
            catch (SerializationException se)
            {
                throw new LJPException($"Bad protocol, problem when deserialize, detail: {se.Message}", LJPExceptionType.BadProtocol);
            }
        }


        /// <summary>
        /// This call need to execute after SendCall<![CDATA[<T>]]> by client or server in theory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public LJPResponse<T> ReceiveResponse<T>(bool bThrowExceptionWithNotData = true)
        {
            var oLJPResponse = ReceiveResponse(new List<Type> { typeof(T) }, bThrowExceptionWithNotData);
            var oTLJPResponse = new LJPResponse<T>
            {
                oObject = oLJPResponse.oObject
            };
            return oTLJPResponse;
        }

        LJPResponse ReceiveResponse(ICollection<Type> tpeNamespace, bool bThrowExceptionWithNotData = true)
        {
            //Receive Header 
            Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
            ClientConnector.Receive(receiveBuffer, 0, receiveBuffer.Length);
            return ConvertToLJPResponse(tpeNamespace, receiveBuffer, bThrowExceptionWithNotData);
        }

        private LJPResponse ConvertToLJPResponse(
              ICollection<Type> tpeNamespace
            , byte[] aReceiveBuffer
            , bool bThrowExceptionWithNotData)
        {

            byte[] dinamycBufferToAllMessage = null;
            try
            {
                LJPResponse oLJPResponse = new LJPResponse();
                var indexOfHeaderAndBodySeparator = ByteHelper.IndexOf(aReceiveBuffer
                                                                            , 0
                                                                            , new byte[] { Convert.ToByte('\n'), Convert.ToByte('\n') });

                if (indexOfHeaderAndBodySeparator == -1)
                {
                    throw new LJPException("Bad Little Json Protocol, Expected Response Object") { LJPExceptionType = LJPExceptionType.BadProtocol };
                }

                #region Extracting Header Of SendResponse Message

                var headerBytes = new byte[indexOfHeaderAndBodySeparator];
                Array.Copy(aReceiveBuffer, headerBytes, indexOfHeaderAndBodySeparator);
                string headerUTF8 = Encoding.UTF8.GetString(headerBytes);
                var headerUTF8Splited = headerUTF8.Split('\n');

                if (headerUTF8Splited.Length < 2 || indexOfHeaderAndBodySeparator == -1)
                {
                    throw new LJPException("Bad Little Json Protocol, Expected Response Object") { LJPExceptionType = LJPExceptionType.BadProtocol };
                }

#if DEBUG
                logger.Debug($"Header=[{string.Join(", ", headerUTF8Splited) }]");
#endif

                //Receive Total of json object
                var lengthAndValueInString = headerUTF8Splited.Single(o => o.Contains("Length="));
                var typeAndValueInString = headerUTF8Splited.Single(o => o.Contains("Type="));

                #endregion

                var lengthInString = lengthAndValueInString.Split(new char[] { '=' }, 2)[1];
                var typeInString = typeAndValueInString.Split(new char[] { '=' }, 2)[1];

                oLJPResponse.iLength = int.Parse(lengthInString);
                try
                {
                    Type tpePrimitive = TryToResolvePrimitivesType(typeInString);
                    if (nameof(LJPExceptionDTO).Equals(typeInString, StringComparison.OrdinalIgnoreCase))
                    {
                        oLJPResponse.tpeObject = reflector.GetType(typeof(LJPExceptionDTO), typeInString);
                    }
                    else if (tpePrimitive == null)
                    {
                        Type tpetryGetType = null;
                        foreach (var tpeNamespaceFE in tpeNamespace)
                        {
                            if (reflector.TryGetType(tpeNamespaceFE, typeInString, out tpetryGetType))
                            {
                                oLJPResponse.tpeObject = tpetryGetType;
                                break;
                            }
                        }
                        if (oLJPResponse.tpeObject == null)
                            throw new ArgumentException($"Type not found in namespaces:\n\"{string.Join(",", tpeNamespace)}\"");
                    }
                    else
                    {
                        oLJPResponse.tpeObject = tpePrimitive;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    throw new LJPException($"Error: trying to get type \"{typeInString}\" in namespace \"{tpeNamespace}\".");
                }

                //Receive the json object part
                int indexOfEndOfBody = ByteHelper.IndexOf(aReceiveBuffer, indexOfHeaderAndBodySeparator + 2, new byte[] { Convert.ToByte('\0') });
                indexOfEndOfBody = indexOfEndOfBody == -1 ? aReceiveBuffer.Length : indexOfEndOfBody;

                var firstExtractOfBody = new byte[indexOfEndOfBody - (indexOfHeaderAndBodySeparator + 2)];
                if (firstExtractOfBody.Length > oLJPResponse.iLength)
                {
                    throw new LJPException($"Bad body length Had={firstExtractOfBody.Length}, Expected={oLJPResponse.iLength}.");
                }

                Buffer.BlockCopy(aReceiveBuffer, indexOfHeaderAndBodySeparator + 2, firstExtractOfBody, 0, firstExtractOfBody.Length);
                int receivedToNow = firstExtractOfBody.Length;

                dinamycBufferToAllMessage = new byte[oLJPResponse.iLength];

                Buffer.BlockCopy(firstExtractOfBody, 0, dinamycBufferToAllMessage, 0, receivedToNow);

                //Whether there is more fragment of the object
                string receivedMessageInASCII = "";
                while (receivedToNow < oLJPResponse.iLength)
                {
                    Array.Clear(aReceiveBuffer, 0, aReceiveBuffer.Length);
                    ClientConnector.Receive(aReceiveBuffer, 0, aReceiveBuffer.Length);

                    int indexOfEnd = ByteHelper.IndexOf(aReceiveBuffer, 0, nullByteInArray);
                    indexOfEnd = indexOfEnd == -1 ? aReceiveBuffer.Length : indexOfEnd;

                    indexOfEnd = Math.Min(oLJPResponse.iLength - receivedToNow, indexOfEnd);

                    Buffer.BlockCopy(aReceiveBuffer, 0, dinamycBufferToAllMessage, receivedToNow, indexOfEnd);

                    receivedToNow += indexOfEnd;
                    logger.Debug($"[ReceivePart={receivedMessageInASCII.Length}] [{receivedToNow}/{oLJPResponse.iLength}]");

                    if (indexOfEnd == 0 || indexOfEnd == -1)
                        break;
                }

                var sObject = Encoding.UTF8.GetString(dinamycBufferToAllMessage);

#if DEBUG
                logger.Debug($"Received Message: [{sObject.Replace("\n", "[nl]")}]");
#endif

                dynamic oObject = null;
                //using (MemoryStream oMS = new())
                //{
                //using StreamWriter oSW = new(oMS);
                //DataContractJsonSerializer oJsonSerializer = new(oLJPResponse.tpeObject);
                //oSW.Write(sObject);
                //oSW.Flush();
                //oMS.Position = 0;
                //oObject = oJsonSerializer.ReadObject(oMS);
                //oLJPResponse.oObject = oObject;
                //}

                oObject = JsonConvert.DeserializeObject(sObject, oLJPResponse.tpeObject);
                oLJPResponse.oObject = oObject;


                if (oObject is LJPExceptionDTO && bThrowExceptionWithNotData)
                {
                    var oLJPException = oObject as LJPExceptionDTO;
                    var nLJPEExceptionType = (LJPExceptionType)oLJPException.IClientLJPExceptionType;
                    var exception = new LJPException(oLJPException.Message, nLJPEExceptionType, oLJPException.Code)
                    {
                        SerializedData = oLJPException.SerializedData
                    };
                    throw exception;
                }
                else if (oObject is LJPExceptionDTO && !bThrowExceptionWithNotData)
                {
                    oLJPResponse.oObject = null;
                }

                return oLJPResponse;
            }
            finally
            {
                dinamycBufferToAllMessage = null;
            }
        }

        private Type TryToResolvePrimitivesType(string v)
        {
            return aPrimitiveTypes.FirstOrDefault(o => o == Type.GetType($"System.{v}"));
        }
        public void Connect(string domain, int port)
        {
            if (!ipsCachedDictionary.ContainsKey(domain) || ipsCachedDictionary[domain].Expire < DateTime.UtcNow)
            {
                var ips = Dns.GetHostAddresses(domain);
                var ipsCached = new LJPDNSCache
                {
                    Ips = ips,
                    Expire = DateTime.UtcNow.AddSeconds(this.options.ExpirationDNSCache),
                    Length = ips.Length,
                    Current = 0
                };
                this.ipsCachedDictionary[domain] = ipsCached;
            }

            var currentIp = this.ipsCachedDictionary[domain].Current;

            var ip = this.ipsCachedDictionary[domain].Ips[currentIp];

            try
            {
                ClientConnector.Connect(ip, port);
            }
            catch (SocketException)
            {
                var ipCached = this.ipsCachedDictionary[domain];
                ipCached.Current = ipCached.Current < (ipCached.Ips.Length - 1) ? ++ipCached.Current : 0;
                throw;
            }
        }

        public void Connect(System.Net.IPAddress ip, int port)
        {

            ClientConnector.Connect(ip, port);
        }
        public void Disconnect(bool dispose = false)
        {
            ClientConnector?.Disconnect(dispose);
            if (dispose)
            {
                this.Dispose();
                this.ClientConnector.Dispose();
            }
        }

        public void Dispose()
        {
            this.receiveBuffer = null;
            Disconnect();
            this.ClientConnector?.Dispose();
        }

        public void SendException(LJPException ex)
        {
            LJPExceptionDTO exc = new()
            {
                IClientLJPExceptionType = (int)ex.LJPExceptionType,
                Message = ex.Message,
                Code = ex.Code,
                SerializedData = ex.SerializedData
            };
            SendResponse(exc);
        }

        public LJPResponse ReceiveResponse(Type tpeNamespace, bool bThrowExceptionWithNoData = true)
        {
            return ReceiveResponse(new List<Type> { tpeNamespace }, bThrowExceptionWithNoData);
        }

        LJPResponse IClientLJP.ReceiveResponse(ICollection<Type> tpeNamespace, bool bThrowExceptionWithNoData)
        {
            throw new NotImplementedException();
        }

        public IClientConnector ClientConnector { get; set; }

        private ClientLJPOptions options;

        public bool KeepAlive
        {
            get; set;

        }
    }
}
