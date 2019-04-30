using NetXP.NetStandard.Auditory;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace NetXP.NetStandard.Network.LittleJsonProtocol.Implementations
{
    public class ClientLJP : IClientLJP
    {
        private readonly IClientConnectorFactory factoryClientTCP;
        private IClientConnector _oIClientTCP;//Not do readonly because is used by other classes.
        private readonly IReflector reflector;
        private readonly ILogger logger;
        private List<Type> aPrimitiveTypes = new List<Type>{
            typeof(string)
            , typeof(int)
            , typeof(int?)
            , typeof(float)
            , typeof(float?)
            , typeof(long)
            , typeof(long?)
            , typeof(double)
            , typeof(double?)
            /// .... etc...
        };

        private byte[] aReceiveBuffer = new byte[5012];
        private byte[] nullByteInArray = new byte[] { 0 };
        private readonly IFactoryClientLJP factoryClientLJP;

        public ClientLJP
        (
              IClientConnectorFactory factoryConnectorFactory
            , IReflector reflector
            , ILogger logger
            , IFactoryClientLJP factoryClientLJP
            )
        {
            this.logger = logger;
            factoryClientTCP = factoryConnectorFactory;
            this.reflector = reflector;
            this.factoryClientLJP = factoryClientLJP;
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
                string sJsonFinal = messageExtractor.Parse(sendCallParameter);

                string message =
                      $"Version={sendCallParameter.Version}\n"
                    + $"Length={Encoding.UTF8.GetBytes(sJsonFinal).Length}\n"
                    + $"Id={sendCallParameter.Id}\n"
                    + $"KeepAlive={sendCallParameter.KeepAlive}\n"
                    + $"NeedResponse={sendCallParameter.NeedResponse}\n"
                    + $"Interface={sendCallParameter.InterfaceName}\n"
                    + $"Method={sendCallParameter.MethodName}\n"
                    + "\n"
                    + $"{sJsonFinal}";

                //logger.Debug($"SendCall To [{ClientTCP.RemoteEndPoint?.ToString() ?? "null"}] => CommmandId = {sendCallParameter?.Id ?? -1},Msg = {sJsonFinal}");

                var aMessage = Encoding.UTF8.GetBytes(message);

                ClientTCP.Send(aMessage, 0, aMessage.Length);
            }
        }

        public void SendResponse(object oObject)
        {
            if (oObject == null)
            {
                oObject = new LJPExceptionDTO() { Message = "No data", IClientLJPExceptionType = 2 };
            }

            var type = oObject.GetType();
            DataContractJsonSerializer oJsonSerializer = new DataContractJsonSerializer(type);
            MemoryStream oMS = new MemoryStream();
            oJsonSerializer.WriteObject(oMS, oObject);//Deserialize the oObject into Memory Stream (oMS).

            oMS.Position = 0;
            string sJson = new StreamReader(oMS).ReadToEnd();//Read object deserialized.
            int iLength = Encoding.UTF8.GetBytes(sJson).Length;

            string message = string.Format(
                  "Length={0}\n"
                + "Type={1}\n"
                + "\n{2}"
                , iLength
                , !oObject.GetType().GetTypeInfo().IsGenericType ? oObject.GetType().Name : oObject.GetType().FullName
                , sJson
            );

            //logger.Debug($"SendReponse [Lenght={iLength}]-[{ClientTCP.RemoteEndPoint.ToString()}] {sJson}");
            var aMessage = Encoding.UTF8.GetBytes(message);


            ClientTCP.Send(aMessage, 0, aMessage.Length);
        }


        public LJPCallReceived ReceiveCall(params string[] servicesLayers)
        {
            byte[] dinamycBufferToAllMessage = null;
            try
            {
                LJPCallReceived oLJPCallResponse = new LJPCallReceived();

                //Receive Header 
                Array.Clear(aReceiveBuffer, 0, aReceiveBuffer.Length);
                ClientTCP.Receive(aReceiveBuffer, 0, aReceiveBuffer.Length);
                var indexOfHeaderAndBodySeparator = ByteHelper.IndexOf(aReceiveBuffer
                                                        , 0
                                                        , new byte[] { Convert.ToByte('\n'), Convert.ToByte('\n') });
                if (indexOfHeaderAndBodySeparator == -1)
                {
                    throw new LJPException("Bad little json protocol, header body separator not found.") { nLJPExceptionType = LJPExceptionType.BadProtocol };
                }

                #region Parsing Raw Data Header Of SendCall Message

                var headerBytes = new byte[indexOfHeaderAndBodySeparator];
                Array.Copy(aReceiveBuffer, headerBytes, indexOfHeaderAndBodySeparator);
                string headerUTF8 = Encoding.UTF8.GetString(headerBytes);
                var headerUTF8Splited = headerUTF8.Split('\n');

                //string message = Encoding.UTF8.GetString(this.aReceiveBuffer);
                logger.Debug($"Header=[{string.Join(", ", headerUTF8Splited) }]");

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
                    ConvertToLJPResponse(new List<Type>() { typeof(LJPExceptionDTO) }, aReceiveBuffer, true);
                }

                if (lengthLine == null
                    || commandIdLine == null
                    || stayAliveLine == null
                    || needResponseLine == null
                    || interfaceLine == null
                    || methodLine == null)
                {
                    throw new LJPException("Bad little json protocol header, all or some fields nulls.") { nLJPExceptionType = LJPExceptionType.BadProtocol };
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
                long commandId = 0;
                if (!long.TryParse(rawCommandId, out commandId))
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

                //Convert to Interface type
                if (serviceInterface == null)
                {
                    throw new LJPException($"Bad little json protocol, Interface or method not found in layers:\n\t{string.Join("\n\t", serviceInterface)}", LJPExceptionType.FormatException);
                }
                oLJPCallResponse.Interface = serviceInterface;

                #endregion

                #region Receiving the json object string part
                int indexOfEndOfBody = ByteHelper.IndexOf(aReceiveBuffer, indexOfHeaderAndBodySeparator + 2, new byte[] { Convert.ToByte('\0') });
                indexOfEndOfBody = indexOfEndOfBody == -1 ? aReceiveBuffer.Length : indexOfEndOfBody;

                var firstExtractOfBody = new byte[indexOfEndOfBody - (indexOfHeaderAndBodySeparator + 2)];
                if (firstExtractOfBody.Length > oLJPCallResponse.iLength)
                {
                    throw new LJPException($"Bad body length Had={firstExtractOfBody.Length}, Expected={oLJPCallResponse.iLength}.");
                }

                Buffer.BlockCopy(aReceiveBuffer, indexOfHeaderAndBodySeparator + 2, firstExtractOfBody, 0, firstExtractOfBody.Length);
                int receivedToNow = firstExtractOfBody.Length;

                dinamycBufferToAllMessage = new byte[oLJPCallResponse.iLength];

                Buffer.BlockCopy(firstExtractOfBody, 0, dinamycBufferToAllMessage, 0, receivedToNow);
                //Whether there is more fragment of the object
                string receivedMessageInASCII = "";
                while (receivedToNow < oLJPCallResponse.iLength)
                {
                    Array.Clear(aReceiveBuffer, 0, aReceiveBuffer.Length);
                    ClientTCP.Receive(aReceiveBuffer, 0, aReceiveBuffer.Length);

                    int indexOfEnd = ByteHelper.IndexOf(aReceiveBuffer, 0, nullByteInArray);
                    indexOfEnd = indexOfEnd == -1 ? aReceiveBuffer.Length : indexOfEnd;

                    Buffer.BlockCopy(aReceiveBuffer, 0, dinamycBufferToAllMessage, receivedToNow, indexOfEnd);
                    receivedToNow += indexOfEnd;

                    logger.Debug($"[ReceivePart={receivedMessageInASCII.Length}] [{receivedToNow}/{oLJPCallResponse.iLength}]");

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
        public LJPResponse<T> ReceiveResponse<T>(bool bThrowExceptionWithNotData = true) where T : class
        {
            var oLJPResponse = ReceiveResponse(new List<Type> { typeof(T) }, bThrowExceptionWithNotData);
            var oTLJPResponse = new LJPResponse<T>();
            oTLJPResponse.oObject = oLJPResponse.oObject as T;
            return oTLJPResponse;
        }

        LJPResponse ReceiveResponse(ICollection<Type> tpeNamespace, bool bThrowExceptionWithNotData = true)
        {
            //Receive Header 
            Array.Clear(aReceiveBuffer, 0, aReceiveBuffer.Length);
            ClientTCP.Receive(aReceiveBuffer, 0, aReceiveBuffer.Length);
            return ConvertToLJPResponse(tpeNamespace, aReceiveBuffer, bThrowExceptionWithNotData);
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
                    throw new LJPException("Bad Little Json Protocol, Expected Response Object") { nLJPExceptionType = LJPExceptionType.BadProtocol };
                }

                #region Extracting Header Of SendResponse Message

                var headerBytes = new byte[indexOfHeaderAndBodySeparator];
                Array.Copy(aReceiveBuffer, headerBytes, indexOfHeaderAndBodySeparator);
                string headerUTF8 = Encoding.UTF8.GetString(headerBytes);
                var headerUTF8Splited = headerUTF8.Split('\n');

                if (headerUTF8Splited.Length < 2 || indexOfHeaderAndBodySeparator == -1)
                {
                    throw new LJPException("Bad Little Json Protocol, Expected Response Object") { nLJPExceptionType = LJPExceptionType.BadProtocol };
                }

                logger.Debug($"Header=[{string.Join(", ", headerUTF8Splited) }]");

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
                    if (tpePrimitive == null)
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
                catch (Exception)
                {
                    oLJPResponse.tpeObject = reflector.GetType(typeof(LJPExceptionDTO), typeInString);
                }
                //Receive the json object part
                //string sObject = string.Join("\n", aMessage.Skip(3));
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
                    ClientTCP.Receive(aReceiveBuffer, 0, aReceiveBuffer.Length);

                    int indexOfEnd = ByteHelper.IndexOf(aReceiveBuffer, 0, nullByteInArray);
                    indexOfEnd = indexOfEnd == -1 ? aReceiveBuffer.Length : indexOfEnd;

                    Buffer.BlockCopy(aReceiveBuffer, 0, dinamycBufferToAllMessage, receivedToNow, indexOfEnd);

                    receivedToNow += indexOfEnd;
                    logger.Debug($"[ReceivePart={receivedMessageInASCII.Length}] [{receivedToNow}/{oLJPResponse.iLength}]");

                    if (indexOfEnd == 0 || indexOfEnd == -1)
                        break;
                }

                var sObject = Encoding.UTF8.GetString(dinamycBufferToAllMessage);

                object oObject = null;
                using (MemoryStream oMS = new MemoryStream())
                {
                    using (StreamWriter oSW = new StreamWriter(oMS))
                    {
                        DataContractJsonSerializer oJsonSerializer = new DataContractJsonSerializer(oLJPResponse.tpeObject);
                        oSW.Write(sObject);
                        oSW.Flush();
                        oMS.Position = 0;
                        oObject = oJsonSerializer.ReadObject(oMS);
                        oLJPResponse.oObject = oObject;
                    }
                }

                if (oObject is LJPExceptionDTO && bThrowExceptionWithNotData)
                {
                    var oLJPException = oObject as LJPExceptionDTO;
                    var nLJPEExceptionType = (LJPExceptionType)oLJPException.IClientLJPExceptionType;
                    throw new LJPException(oLJPException.Message) { nLJPExceptionType = nLJPEExceptionType };
                }
                //else
                //{
                //    oLJPResponse.oObject = null;
                //}

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

        public void Connect(System.Net.IPAddress oIPAddress, int iPort)
        {
            ClientTCP = factoryClientTCP.Create();
            ClientTCP.Connect(oIPAddress, iPort);
        }
        public void Disconnect(bool dispose = true)
        {
            //this.bKeepAlive = false;
            ClientTCP?.Disconnect(dispose);
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void SendException(NetXP.NetStandard.Network.LittleJsonProtocol.LJPException ex)
        {
            LJPExceptionDTO exc = new LJPExceptionDTO
            {
                IClientLJPExceptionType = (int)ex.nLJPExceptionType,
                Message = ex.Message
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

        public IClientConnector ClientTCP
        {
            get
            {
                return _oIClientTCP;
            }
            set
            {
                _oIClientTCP = value;
            }
        }


        public bool KeepAlive
        {
            get; set;

        }
    }
}
