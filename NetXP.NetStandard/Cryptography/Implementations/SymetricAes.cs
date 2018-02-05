using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NetXP.NetStandard.Cryptography;

namespace NetXP.NetStandard.Cryptography.Implementations
{
    public class SymetricAes : ISymetricCrypt
    {
        public CipherMode Mode { get; private set; }

        public SymetricAes()
        {
            this.Mode = CipherMode.ECB;
        }



        //T could be: 
        //      AesManaged
        //		TripleDESCryptoServiceProvider
        //		RijndaelManaged
        public byte[] Encrypt(byte[] aNoEncryptedMessage, SymetricKey SymetricKey)
        {

            SymmetricAlgorithm algorithm = Aes.Create();
            algorithm.Key = SymetricKey.Key;
            algorithm.IV = SymetricKey.Salt;
            algorithm.Mode = this.Mode;
            //algorithm.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = algorithm.CreateEncryptor(SymetricKey.Key, SymetricKey.Salt);

            using (MemoryStream buffer = new MemoryStream())
            {
                using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
                {
                    stream.Write(aNoEncryptedMessage, 0, aNoEncryptedMessage.Length);
                }
                return buffer.ToArray();
            }
        }

        public byte[] Decrypt(byte[] aEncryptedMessage, SymetricKey SymetricKey)
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(SymetricKey.Key, SymetricKey.Salt, SymetricKey.Iteration);

            SymmetricAlgorithm algorithm = Aes.Create();
            algorithm.Key = SymetricKey.Key;
            algorithm.IV = SymetricKey.Salt;

            algorithm.Mode = this.Mode;
            //algorithm.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = algorithm.CreateDecryptor(SymetricKey.Key, SymetricKey.Salt);

            using (MemoryStream buffer = new MemoryStream(aEncryptedMessage))
            using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
            using (MemoryStream mo = new MemoryStream())
            {
                stream.CopyTo(mo);
                mo.Position = 0;
                return mo.ToArray();
            }
        }

        public SymetricKey Generate()
        {
            var symetricKey = new SymetricKey();
            SymmetricAlgorithm algorithm = Aes.Create();
            symetricKey.Key = algorithm.Key;
            symetricKey.Salt = algorithm.IV;
            return symetricKey;
        }
    }
}