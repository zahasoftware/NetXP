using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Serialization
{
    /// <summary>
    /// Deserialize or serialize a generic class in bytes
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Deserialise the byte array to T generic class.
        /// </summary>
        /// <typeparam name="T">Generic clase type</typeparam>
        /// <param name="bytesToDeserialize">Array of bytes to deserialize</param>
        /// <returns>Generic class.</returns>
        T Deserialize<T>(byte[] bytesToDeserialize) where T : class;

        /// <summary>
        /// Serialize T object to byte array.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="objectToSerialize">Object instance to serialize</param>
        /// <returns></returns>
        byte[] Serialize<T>(T objectToSerialize) where T : class;


        /// <summary>
        /// Deserialize a byte array to specified type 
        /// </summary>
        /// <param name="type">type object</param>
        /// <param name="bytesToDeserialize">bytes to deserialize</param>
        /// <returns>Boxing deserialize object of specified type</returns>
        object Deserialize(Type type, byte[] bytesToDeserialize);
    }
}
