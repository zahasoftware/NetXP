using NetXP.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.Serialization.Implementations
{
    public static class SerializationCompositionRoot
    {
        public static void RegisterSerialization(this IRegister register)
        {
            register.Register<ISerializerFactory, SerializeTFactory>(DILifeTime.Singleton);
            register.Register<ISerializer, Serialize2Xml>(SerializerType.Xml.ToString(), DILifeTime.Singleton);
            register.Register<ISerializer, Serialize2Json>(SerializerType.Json.ToString(), DILifeTime.Singleton);
            register.Register<ISerializer, JsonConverterSerializer>(SerializerType.JsonConvert.ToString(), DILifeTime.Singleton);
            register.Register<ISerializer, Serializer2XmlWithXmlSerializer>(SerializerType.XmlSerializer.ToString(), DILifeTime.Singleton);
            register.Register<ISerializer, Serialize2Json>(DILifeTime.Singleton);
        }
    }
}
