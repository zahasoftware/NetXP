using NetXP.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network
{
    public interface ISecureProtocolHandshake
    {
        PublicKey GetFirstPublickKey();
        PrivateKey GetFirstPrivateKey();
    }
}