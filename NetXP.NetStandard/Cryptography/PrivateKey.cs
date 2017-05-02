using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Cryptography
{
    /// <summary>
    /// Private Key Contain a public key
    /// This class is used with IAsymetricCrypt
    /// </summary>
    public class PrivateKey
    {
        public byte[] yD { get; set; }
        public byte[] yQ { get; set; }
        public byte[] yP { get; set; }
        public byte[] yModulus { get; set; }
        public byte[] yExponent { get; set; }
        public byte[] yDQ { get; set; }
        public byte[] yDP { get; set; }
        public byte[] yInverseQ { get; set; }
    }
}
