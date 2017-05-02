using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Cryptography.Implementations
{
    public class HashSHA256 : IHash
    {
        private readonly SHA256 sha256;
        public HashSHA256()
        {
            sha256 = System.Security.Cryptography.SHA256.Create();
        }

        public void Dispose()
        {
            sha256.Dispose();
        }

        public byte[] Generate(ByteArray ByteArray)
        {
            return sha256.ComputeHash(ByteArray.Bytes, ByteArray.Offset, ByteArray.Length);
        }
    }
}
