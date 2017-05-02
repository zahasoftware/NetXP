using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Cryptography
{
    public interface IHash
    {
        byte[] Generate(ByteArray byteArray);
    }
}
