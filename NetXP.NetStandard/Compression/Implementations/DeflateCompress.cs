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
        /// <summary>
        /// Compress the bytes passed as argument from offset to length of specified bytes
        /// </summary>
        /// <param name="bytesToCompress">Object with bytes array</param>
        /// <returns></returns>
        public byte[] Compress(ByteArray bytesToCompress)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(bytesToCompress.Bytes, bytesToCompress.Offset, bytesToCompress.Length);
            }
            return output.ToArray();
        }

        /// <summary>
        /// Compress all bytes passed as argument
        /// </summary>
        /// <param name="bytesToCompress">bytes to compress</param>
        /// <returns></returns>
        public byte[] Compress(byte[] bytesToCompress)
        {
            return this.Compress(new ByteArray(bytesToCompress));
        }

        /// <summary>
        /// Decompress bytes from offset to length of specified bytes in object ByteArray
        /// </summary>
        /// <param name="bytesToDecompress">Object with bytes</param>
        /// <returns></returns>
        public byte[] Decrompress(ByteArray bytesToDecompress)
        {
            MemoryStream input = new MemoryStream(bytesToDecompress.OffsettedBytes);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        public byte[] Decrompress(byte[] compressedBytes)
        {
            return this.Decrompress(new ByteArray(compressedBytes));
        }
    }
}
