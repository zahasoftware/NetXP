using System;
using System.Linq;

namespace NetXP
{
    public class BitHelper
    {
        //http://stackoverflow.com/questions/18145667/how-can-i-reverse-the-byte-order-of-an-int
        /// <summary> * * 
        /// >> 8*3	    >> 8	  << 8	  << 8*3	
        /// ----------------------------------------
        ///    1	      2	        3	      4	    
        /// 
        // -----------------------------------------
        ///    4	      3	        2	      1	    
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint ReverseBytes(ushort value)
        {
            return (value & 0x00FFU) << 8  // One Byte to Left
                 | (value & 0xFF00U) >> 8; // ... 
        }

        //http://stackoverflow.com/questions/18145667/how-can-i-reverse-the-byte-order-of-an-int
        /// <summary> * * 
        /// >> 8*3	    >> 8	  << 8	  << 8*3	
        /// ----------------------------------------
        ///    1	      2	        3	      4	    
        /// 
        // -----------------------------------------
        ///    4	      3	        2	      1	    
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint ReverseBytes(uint value)
        {
            return (value & 0x000000FFU) << (8 * 3) //Three Bytes to left
                 | (value & 0x0000FF00U) << 8  // One Byte to Left
                 | (value & 0x00FF0000U) >> 8 // ... 
                 | (value & 0xFF000000U) >> (8 * 3); // ...
        }

        /// <summary>
        /// >> 8*7	    >> 8*5 	    >> 8*3	    >> 8	  << 8	  << 8*3	  <<8*5	    << 8*7
        ///    Before:
        /// ----------------------------------------------------------------------------------
        ///     1	        2	       3	      4	        5	      6	        7	        8
		///    Result:				
        // ----------------------------------------------------------------------------------
        ///     8	        7	       6	      5	        4	      3	        2	        1
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ulong ReverseBytes(ulong value)
        {
            return
                //To Left
                (value & 0x00000000000000FFU) << (8 * 7)
              | (value & 0x000000000000FF00U) << (8 * 5)
              | (value & 0x0000000000FF0000U) << (8 * 3)
              | (value & 0x00000000FF000000U) << 8
              //To Right
              | (value & 0x000000FF00000000U) >> 8
              | (value & 0x0000FF0000000000U) >> (8 * 3)
              | (value & 0x00FF000000000000U) >> (8 * 5)
              | (value & 0xFF00000000000000U) >> (8 * 7);
        }

        public static int ToInt32(byte[] bytes, int iIndex)
        {
            var int32 = bytes.Skip(iIndex).Take(sizeof(int)).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(int32, 0, sizeof(int));

            return BitConverter.ToInt32(int32, 0);
        }

        public static long ToInt64(byte[] bytes, int index)
        {
            var int64 = bytes.Skip(index).Take(sizeof(long)).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(int64, 0, sizeof(long));

            return BitConverter.ToInt32(int64, 0);
        }

        public static short ToInt16(byte[] bytes, int index)
        {
            var value = bytes.Skip(index).Take(sizeof(short)).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(value, 0, sizeof(short));

            return BitConverter.ToInt16(value, 0);
        }
    }
}
