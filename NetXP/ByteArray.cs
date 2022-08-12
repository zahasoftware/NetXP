using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP
{
    public class ByteArray
    {
        /// <summary>
        /// Make a instance with bytes with offset 0 and length of byte
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        public ByteArray(byte[] bytes)
        {
            this.bytes = bytes;
            this.Length = bytes.Length;
            offsetBytes = bytes;
        }

        /// <summary>
        /// Make a instance with bytes and offset equals to 0 and specified length
        /// </summary>
        /// <param name="length">Length from 0 index</param>
        public ByteArray(int length)
        {
            this.bytes = new byte[length];
            this.Length = length;
            offsetBytes = bytes.Take(length).ToArray();
        }

        /// <summary>
        /// Make a instance with specified bytes and offset and length.
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <param name="offset">Offset index on bytes</param>
        /// <param name="length">Length from offset index</param>
        public ByteArray(byte[] bytes, int offset, int length) : this(bytes)
        {
            this.Offset = offset;
            this.Length = length;
            this.bytes = bytes;
            offsetBytes = bytes.Skip(offset).Take(length).ToArray();
        }

        public int Offset { get; private set; }
        public int Length { get; private set; }

        private byte[] bytes;
        private byte[] offsetBytes;

        /// <summary>
        /// Bytes passed as parameter in constructor
        /// </summary>
        public byte[] Bytes
        {
            get { return bytes; }
        }

        /// <summary>
        /// Return offsetted bytes array "from Bytes Skip Offset And Take Length"
        /// </summary>
        public byte[] OffsettedBytes { get => offsetBytes; }

    }
}
