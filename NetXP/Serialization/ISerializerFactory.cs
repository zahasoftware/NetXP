using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer Resolve(SerializerType serializeType);
    }
}