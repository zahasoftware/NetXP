using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Compression
{
    public interface ICompression
    {
        /// <summary>
        /// Compress a byte of array
        /// </summary>
        /// <param name="aInputBuffer">Buffer where is data to compress</param>
        byte[] Compress(ByteArray bytesToCompress);
        byte[] Compress(byte[] bytesToCompress);

        /// <summary>
        /// Decompress a byte of array
        /// </summary>
        /// <param name="aOutputBuffer">Data to decompress</param>
        byte[] Decrompress(ByteArray compressedBytes);


        byte[] Decrompress(byte[] compressedBytes);

    }
}
