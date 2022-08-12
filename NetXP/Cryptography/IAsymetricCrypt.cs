using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NetXP.Cryptography
{
    public interface IAsymetricCrypt : IDisposable
    {

        /// <summary>
        /// Desencripta el contenido con la clave privada generada.
        /// </summary>
        byte[] Decrypt(byte[] encryptedBytes);

        /// <summary>
        /// Encripta el contenido con la clave publica.
        /// </summary>
        /// <param name="message"></param>
        byte[] Encrypt(byte[] decryptedBytes);

        /// <summary>
        /// Public key of remote host.
        /// </summary>
        /// <param name="sKey"></param>
        PrivateKey GetPrivateKey();

        PublicKey GetPublicKey();

        void SetPrivateKey(PrivateKey privateKey);
        void SetPublicKey(PublicKey publicKey);

        void GenerateKeys();
    }
}
