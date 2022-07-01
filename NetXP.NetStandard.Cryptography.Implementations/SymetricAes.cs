using System.IO;
using System.Security.Cryptography;

namespace NetXP.Cryptography.Implementations
{
    public class SymetricAes : ISymetricCrypt
    {
        public CipherMode Mode { get; private set; }

        public SymetricAes()
        {
            Mode = CipherMode.ECB;
        }

        //T could be: 
        //      AesManaged
        //		TripleDESCryptoServiceProvider
        //		RijndaelManaged
        public byte[] Encrypt(byte[] noEncryptedMessage, SymetricKey symetricKey)
        {

            SymmetricAlgorithm algorithm = Aes.Create();
            algorithm.Key = symetricKey.Key;
            algorithm.IV = symetricKey.Salt;
            algorithm.Mode = Mode;
            //algorithm.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = algorithm.CreateEncryptor(symetricKey.Key, symetricKey.Salt);

            using (MemoryStream buffer = new())
            {
                using (CryptoStream stream = new(buffer, transform, CryptoStreamMode.Write))
                {
                    stream.Write(noEncryptedMessage, 0, noEncryptedMessage.Length);
                    stream.FlushFinalBlock();//Error:https://stackoverflow.com/a/40564155
                }
                return buffer.ToArray();
            }
        }

        public byte[] Decrypt(byte[] encryptedMessage, SymetricKey symetricKey)
        {
            SymmetricAlgorithm algorithm = Aes.Create();
            algorithm.Key = symetricKey.Key;
            algorithm.IV = symetricKey.Salt;
            algorithm.Mode = Mode;

            ICryptoTransform transform = algorithm.CreateDecryptor(symetricKey.Key, symetricKey.Salt);

            using MemoryStream buffer = new(encryptedMessage);
            using CryptoStream stream = new(buffer, transform, CryptoStreamMode.Read);
            using MemoryStream mo = new();
            stream.CopyTo(mo);
            mo.Position = 0;
            return mo.ToArray();
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