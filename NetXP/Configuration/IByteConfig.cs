using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Configuration
{
    public interface IByteConfig
    {
        byte[] ReadToByte();

        void WriteBytes(byte[] aBytes);
    }
}
