using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Serialization.Implementations
{
    public class SerializeTFactory : ISerializerFactory
    {
        private readonly IContainer uc;

        public SerializeTFactory(IContainer uc)
        {
            this.uc = uc;
        }

        public ISerializer Resolve(SerializerType serializeType = SerializerType.Json)
        {
            if (SerializerType.Xml == serializeType)
                return this.uc.Resolve<ISerializer>(SerializerType.Xml.ToString());
            else
                return this.uc.Resolve<ISerializer>(SerializerType.Json.ToString());
        }
    }
}
