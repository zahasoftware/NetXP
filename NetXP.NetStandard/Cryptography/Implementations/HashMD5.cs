using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Cryptography.Implementations
{
    public class HashMD5 : IHash, IDisposable
    {
        private readonly MD5 md5;
        public HashMD5()
        {
            md5 = System.Security.Cryptography.MD5.Create();
        }

        public void Dispose()
        {
            md5.Dispose();
        }

        public byte[] Generate(ByteArray ByteArray)
        {
            return md5.ComputeHash(ByteArray.Bytes, ByteArray.Offset, ByteArray.Length);
        }
    }
}
