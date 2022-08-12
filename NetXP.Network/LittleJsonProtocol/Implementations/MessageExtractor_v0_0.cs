using System;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace NetXP.Network.LittleJsonProtocol.Implementations
{
    public class MessageExtractor_v0_0 : ILJPMessageFactory
    {
        List<Type> ignoredTypes = new List<Type> { typeof(IClientLJP) };

        public string Parse(LJPCall sendCallParameter)
        {
            string sJsonFinal = "";
            string[] aParamsType = null;
            using (MemoryStream oMS = new MemoryStream())
            {
                using (StreamWriter oSW = new StreamWriter(oMS))
                {
                    //Desearilizing Parameters
                    string[] aParamsSerialized = new string[sendCallParameter.Parameters.Count];
                    aParamsType = new string[sendCallParameter.Parameters.Count];
                    for (int i = 0; i < sendCallParameter.Parameters.Count; i++) //Type tpeParameterFE in oDTOLJPResponse.aParametersType)
                    {
                        DataContractJsonSerializer jsonSerializerParameter = new DataContractJsonSerializer(
                            sendCallParameter.Parameters[i].GetType());
                        jsonSerializerParameter.WriteObject(oMS, sendCallParameter.Parameters[i]);//Write object in memory stream

                        oMS.Position = 0;
                        string sJson = new StreamReader(oMS).ReadToEnd();//read memory stream of serialized object

                        aParamsType[i] = sendCallParameter.Parameters[i].GetType().Name;
                        aParamsSerialized[i] = sJson;//Output array
                    }

                    oMS.Position = 0;
                    DataContractJsonSerializer jsonSerializerParametesArray = new DataContractJsonSerializer(typeof(string[]));
                    jsonSerializerParametesArray.WriteObject(oMS, aParamsSerialized);
                    oMS.Position = 0;
                    sJsonFinal = new StreamReader(oMS).ReadToEnd();
                }
            }

            return sJsonFinal;
        }

        public string Parse(LJPCallReceived oLJPCallResponse, string sObject)
        {
            var methodParameters = oLJPCallResponse.Method.GetParameters();
            using (MemoryStream oMS = new MemoryStream())
            {
                using (StreamWriter oSW = new StreamWriter(oMS))
                {

                    DataContractJsonSerializer jsonSerializerParametesArray = new DataContractJsonSerializer(typeof(string[]));
                    oSW.Write(sObject.Trim().Replace("\0", ""));
                    oSW.Flush();
                    oMS.Position = 0;
                    var aObject = jsonSerializerParametesArray.ReadObject(oMS) as string[];

                    //Desearilizing Parameters
                    for (int i = 0; methodParameters != null && i < methodParameters.Length; i++) //Type tpeParameterFE in oDTOLJPResponse.aParametersType)
                    {
                        if (ignoredTypes.Any(o => o == methodParameters[i].ParameterType))
                        {
                            continue;
                        }

                        Type tpeParameterFE = methodParameters[i].GetType();
                        sObject = aObject[i];
                        DataContractJsonSerializer jsonSerializerParameter = new DataContractJsonSerializer(tpeParameterFE);
                        oSW.Write(sObject);
                        oSW.Flush();
                        oMS.Position = 0;
                        var oParameter = jsonSerializerParameter.ReadObject(oMS);
                        oLJPCallResponse.Parameters.Add(oParameter);
                    }
                }
            }

            return sObject;
        }

    }
}

