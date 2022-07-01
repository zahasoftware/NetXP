using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Cryptography.Implementations
{
    public class HashBase : IHash, IDisposable
    {
        private readonly HashAlgorithm hash;

        public HashBase(HashAlgorithm hash)
        {
            this.hash = hash;
        }

        public void Dispose()
        {
            hash.Dispose();
        }

        public byte[] Generate(ByteArray ByteArray)
        {
            return hash.ComputeHash(ByteArray.Bytes, ByteArray.Offset, ByteArray.Length);
        }

        public byte[] Generate(byte[] bytes)
        {
            return hash.ComputeHash(bytes, 0, bytes.Length);
        }

        public string GenerateToString(byte[] bytes)
        {
            var r = hash.ComputeHash(bytes, 0, bytes.Length);
            return BitConverter.ToString(r).Replace("-", "");
        }
    }
}
