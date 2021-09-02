using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace NetXP.NetStandard.Network.LittleJsonProtocol.Implementations
{
    public class MessageExtractor_v2_0 : ILJPMessageFactory
    {
        List<Type> ignoredTypes = new List<Type> { typeof(IClientLJP) };

        public string Parse(LJPCall sendCallParameter)
        {
            //Desearilizing Parameters
            string[] aParamsSerialized = new string[sendCallParameter.Parameters.Count];
            var aParamsType = new string[sendCallParameter.Parameters.Count];

            for (int i = 0; i < sendCallParameter.Parameters.Count; i++) //Type tpeParameterFE in oDTOLJPResponse.aParametersType)
            {
                string json = JsonConvert.SerializeObject(sendCallParameter.Parameters[i]);
                aParamsType[i] = sendCallParameter.Parameters[i].GetType().Name;
                aParamsSerialized[i] = json;//Output array
            }

            LJPCallMessageDTO ljpCallMessageDTO = new()
            {
                Parameters = aParamsSerialized,
                Credential = new LJPCredentialDTO
                {
                    Identifiers = sendCallParameter.Credential?.Identifier,
                    Roles = sendCallParameter.Credential?.Roles,
                }
            };


            return JsonConvert.SerializeObject(ljpCallMessageDTO); ;
        }

        public string Parse(LJPCallReceived oLJPCallResponse, string sObject)
        {
            var methodParameters = oLJPCallResponse.Method.GetParameters();
            var ljpCallMessageDTO = JsonConvert.DeserializeObject<LJPCallMessageDTO>(sObject.Trim().Replace("\0", ""));

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

            return sObject;
        }
    }
}
