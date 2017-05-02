using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Serialization
{
    public interface IFactorySerializer
    {
        ISerializer Resolve(SerializerType serializeType);
    }
}
