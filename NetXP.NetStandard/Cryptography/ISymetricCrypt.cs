using NetXP.NetStandard.Cryptography.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Cryptography
{
    public interface ISymetricCrypt
    {
        SymetricKey Generate();
        byte[] Encrypt(byte[] decryptedBytes, SymetricKey symetricKey);
        byte[] Decrypt(byte[] encryptedBytes, SymetricKey symetricKey);
    }
}
