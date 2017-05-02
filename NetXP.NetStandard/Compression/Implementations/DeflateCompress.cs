using NetXP.NetStandard.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Compression.Implementations
{
    public class DeflateCompress : ICompression
    {
        public byte[] Compress(ByteArray bytesToCompress)
        {
            using (MemoryStream msin = new MemoryStream(bytesToCompress.Bytes, bytesToCompress.Offset, bytesToCompress.Length))
            using (MemoryStream ms = new MemoryStream())
            using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
            {
                msin.Position = 0;
                msin.CopyTo(ds);

                return ms.ToArray();
            }
        }

        public byte[] Compress(byte[] bytesToCompress)
        {
            return this.Compress(new ByteArray(bytesToCompress));
        }

        public byte[] Decrompress(ByteArray bytesToDecompress)
        {
            using (MemoryStream ms = new MemoryStream(bytesToDecompress.Bytes, bytesToDecompress.Offset, bytesToDecompress.Length))
            using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
            {
                using (MemoryStream mso = new MemoryStream())
                {
                    ds.CopyTo(mso);
                    mso.Position = 0;
                    return mso.ToArray();
                }
            }
        }

        public byte[] Decrompress(byte[] compressedBytes)
        {
            return this.Decrompress(new ByteArray(compressedBytes));
        }
    }
}
