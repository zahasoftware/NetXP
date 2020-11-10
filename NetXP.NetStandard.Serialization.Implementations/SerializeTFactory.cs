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
            return this.uc.Resolve<ISerializer>(serializeType.ToString());///XmlSerializer
        }
    }
}
