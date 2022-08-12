using System;

namespace NetXP
{
    public static class ByteHelper
    {
        public static void UIntToByte(byte[] outputBytes, int index, uint input)
        {
            if (BitConverter.IsLittleEndian)
            {
                BitHelper.ReverseBytes(input);
            }

            byte decre = 4;
            for (int c = 0; c < 4; c++)
            {
                outputBytes[c + index] = (byte)(input >> (8 * --decre));
            }
        }

        public static void UInt16ToByte(byte[] outputBytes, int index, ushort input)
        {
            if (BitConverter.IsLittleEndian)
            {
                BitHelper.ReverseBytes(input);
            }

            byte decre = 2;
            for (int c = 0; c < 2; c++)
            {
                outputBytes[c + index] = (byte)(input >> (8 * --decre));
            }
        }

        public static void UInt32ToByte(byte[] outputBytes, int index, uint input)
        {
            if (BitConverter.IsLittleEndian)
            {
                BitHelper.ReverseBytes(input);
            }

            byte decre = 4;
            for (int c = 0; c < 4; c++)
            {
                outputBytes[c + index] = (byte)(input >> (8 * --decre));
            }
        }

        public static void UInt64ToByte(byte[] outputBytes, int index, ulong input)
        {
            if (BitConverter.IsLittleEndian)
            {
                BitHelper.ReverseBytes(input);
            }

            byte decre = 8;
            for (int c = 0; c < 8; c++)
            {
                outputBytes[c + index] = (byte)(input >> (8 * --decre));
            }
        }

        /// <summary>Looks for the next occurrence of a sequence in a byte array</summary>
        /// <param name="array">Array that will be scanned</param>
        /// <param name="start">Index in the array at which scanning will begin</param>
        /// <param name="sequence">Sequence the array will be scanned for</param>
        /// <returns>
        ///   The index of the next occurrence of the sequence of -1 if not found
        /// </returns>
        public static int IndexOf(byte[] array, int start, byte[] sequence)
        {
            int end = array.Length - sequence.Length; // past here no match is possible
            byte firstByte = sequence[0]; // cached to tell compiler there's no aliasing

            while (start < end)
            {
                // scan for first byte only. compiler-friendly.
                if (array[start] == firstByte)
                {
                    // scan for rest of sequence
                    for (int offset = 0; offset < sequence.Length; ++offset)
                    {
                        if (array[start + offset] != sequence[offset])
                        {
                            break; // mismatch? continue scanning with next byte
                        }
                        else if (offset == sequence.Length - 1)
                        {
                            return start; // all bytes matched!
                        }
                    }

                }
                ++start;
            }

            // end of array reached without match
            return -1;
        }
    }
}
