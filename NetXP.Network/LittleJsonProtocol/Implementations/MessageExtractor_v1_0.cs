using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace NetXP.Network.LittleJsonProtocol.Implementations
{
    public class MessageExtractor_v1_0 : ILJPMessageFactory
    {
        List<Type> ignoredTypes = new List<Type> { typeof(IClientLJP) };

        public string Parse(LJPCall sendCallParameter)
        {
            string jsonMessageSerialized = "";
            string[] aParamsType = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    //Desearilizing Parameters
                    string[] aParamsSerialized = new string[sendCallParameter.Parameters.Count];
                    aParamsType = new string[sendCallParameter.Parameters.Count];

                    for (int i = 0; i < sendCallParameter.Parameters.Count; i++) //Type tpeParameterFE in oDTOLJPResponse.aParametersType)
                    {
                        DataContractJsonSerializer jsonSerializerParameter = new DataContractJsonSerializer(
                            sendCallParameter.Parameters[i].GetType());
                        jsonSerializerParameter.WriteObject(ms, sendCallParameter.Parameters[i]);//Write object in memory stream

                        ms.Position = 0;
                        string sJson = new StreamReader(ms).ReadToEnd();//read memory stream of serialized object

                        aParamsType[i] = sendCallParameter.Parameters[i].GetType().Name;
                        aParamsSerialized[i] = sJson;//Output array
                    }

                    LJPCallMessageDTO ljpCallMessageDTO = new LJPCallMessageDTO
                    {
                        Parameters = aParamsSerialized,
                        Credential = new LJPCredentialDTO
                        {
                            Identifiers = sendCallParameter.Credential?.Identifier,
                            Roles = sendCallParameter.Credential?.Roles,
                        }
                    };

                    ms.Position = 0;
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(LJPCallMessageDTO));
                    jsonSerializer.WriteObject(ms, ljpCallMessageDTO);
                    ms.Position = 0;
                    jsonMessageSerialized = new StreamReader(ms).ReadToEnd();
                }
            }

            return jsonMessageSerialized;
        }

        public string Parse(LJPCallReceived oLJPCallResponse, string sObject)
        {
            var methodParameters = oLJPCallResponse.Method.GetParameters();
            using (MemoryStream oMS = new MemoryStream())
            {
                using (StreamWriter oSW = new StreamWriter(oMS))
                {
                    DataContractJsonSerializer jsonSerializerParametesArray = new DataContractJsonSerializer(typeof(LJPCallMessageDTO));
                    oSW.Write(sObject.Trim().Replace("\0", ""));
                    oSW.Flush();
                    oMS.Position = 0;
                    var ljpCallMessageDTO = jsonSerializerParametesArray.ReadObject(oMS) as LJPCallMessageDTO;

                    oLJPCallResponse.Credential = new LJPCredential
                    {
                        Identifier = ljpCallMessageDTO.Credential?.Identifiers,
                        Roles = ljpCallMessageDTO.Credential?.Roles
                    };

                    if (ljpCallMessageDTO.Parameters == null && methodParameters != null && methodParameters.Length > 0)
                    {
                        throw new LJPException($"Bad LJP message without parameters, expected={methodParameters.Length}");
                    }

                    //Desearilizing Parameters
                    for (int i = 0; methodParameters != null && i < methodParameters.Length; i++) //Type tpeParameterFE in oDTOLJPResponse.aParametersType)
                    {
                        if (ignoredTypes.Any(o => o == methodParameters[i].ParameterType))
                        {
                            continue;
                        }
                        Type tpeParameterFE = methodParameters[i].ParameterType;
                        sObject = ljpCallMessageDTO.Parameters[i];
                        var oParameter = JsonConvert.DeserializeObject(sObject, tpeParameterFE);
                        oLJPCallResponse.Parameters.Add(oParameter);
                    }
                }
            }

            return sObject;
        }
    }
}
