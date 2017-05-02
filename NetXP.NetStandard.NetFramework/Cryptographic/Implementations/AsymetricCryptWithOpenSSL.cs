using OpenSSL.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetXP.NetStandard.Cryptography;

namespace NetXP.NetStandard.NetFramework.Cryptography.Implementations
{
    public class AsymetricCryptWithOpenSSL : IAsymetricCrypt, IDisposable
    {
        private readonly RSA rsa;
        private static object rsaLock = new object();

        public AsymetricCryptWithOpenSSL()
        {
            rsa = new RSA();
        }

        public byte[] Decrypt(byte[] encryptedBytes)
        {
            return rsa.PrivateDecrypt(encryptedBytes, RSA.Padding.PKCS1);
        }

        public byte[] Encrypt(byte[] decryptedBytes)
        {
            return rsa.PublicEncrypt(decryptedBytes, RSA.Padding.PKCS1);
        }

        public PrivateKey GetPrivateKey()
        {
            rsa.Check();

            if (privKey == null)
            {

                byte[] yQ = new byte[rsa.SecretPrimeFactorQ.Bytes];//Prime Factor
                byte[] yP = new byte[rsa.SecretPrimeFactorQ.Bytes];//Prime Factor

                byte[] yInverseQ = new byte[rsa.IQmodP.Bytes];//Prime Factor
                byte[] yDP = new byte[rsa.DmodP1.Bytes];
                byte[] yDQ = new byte[rsa.DmodQ1.Bytes];

                //Public Part Key
                byte[] yPm = new byte[rsa.PublicModulus.Bytes];
                byte[] yPe = new byte[rsa.PublicExponent.Bytes];
                byte[] yD = new byte[rsa.PrivateExponent.Bytes];

                privKey = new PrivateKey()
                {
                    yQ = yQ,
                    yP = yP,
                    yInverseQ = yInverseQ,
                    yDP = yDP,
                    yDQ = yDQ,
                    yModulus = yPm,
                    yExponent = yPe,
                    yD = yD,
                };

                rsa.SecretPrimeFactorQ.ToBytes(privKey.yQ);
                rsa.SecretPrimeFactorP.ToBytes(privKey.yP);
                rsa.IQmodP.ToBytes(privKey.yInverseQ);
                rsa.DmodP1.ToBytes(privKey.yDP);
                rsa.DmodQ1.ToBytes(privKey.yDQ);
                rsa.PublicModulus.ToBytes(privKey.yModulus);
                rsa.PublicExponent.ToBytes(privKey.yExponent);
                rsa.PrivateExponent.ToBytes(privKey.yD);
            }
            return privKey;
        }

        PrivateKey privKey;
        PublicKey publicKey;
        public PublicKey GetPublicKey()
        {

            if (publicKey == null)
            {
                byte[] yPm = new byte[rsa.PublicModulus.Bytes];
                byte[] yPe = new byte[rsa.PublicExponent.Bytes];

                publicKey = new PublicKey()
                {
                    yModulus = yPm,
                    yExponent = yPe,
                };

                rsa.PublicModulus.ToBytes(publicKey.yModulus);
                rsa.PublicExponent.ToBytes(publicKey.yExponent);
            }
            return publicKey;
        }

        void IAsymetricCrypt.GenerateKeys()
        {
            lock (rsaLock)
            {
                rsa.GenerateKeys(2048, 0x10001, null, null);
                privKey = null;
                this.publicKey = null;
            }
        }

        public void Dispose()
        {
            this.rsa.Dispose();
            this.privKey = null;
            this.publicKey = null;
        }

        public void SetPrivateKey(PrivateKey privateKey)
        {
            if (privateKey.yQ == null || privateKey.yP == null)
                throw new Exception("Invalid Private Key.");

            rsa.SecretPrimeFactorQ = OpenSSL.Core.BigNumber.FromArray(privateKey.yQ);
            rsa.SecretPrimeFactorP = OpenSSL.Core.BigNumber.FromArray(privateKey.yP);
            rsa.IQmodP = OpenSSL.Core.BigNumber.FromArray(privateKey.yInverseQ);
            rsa.DmodP1 = OpenSSL.Core.BigNumber.FromArray(privateKey.yDP);
            rsa.DmodQ1 = OpenSSL.Core.BigNumber.FromArray(privateKey.yDQ);
            rsa.PublicModulus = OpenSSL.Core.BigNumber.FromArray(privateKey.yModulus);
            rsa.PublicExponent = OpenSSL.Core.BigNumber.FromArray(privateKey.yExponent);
            rsa.PrivateExponent = OpenSSL.Core.BigNumber.FromArray(privateKey.yD);

            this.privKey = privateKey;
        }

        public void SetPublicKey(PublicKey publicKey)
        {
            if (publicKey.yModulus == null || publicKey.yExponent == null)
            {
                throw new Exception("Invalid Public Key.");
            }

            rsa.PublicModulus = OpenSSL.Core.BigNumber.FromArray(publicKey.yModulus);
            rsa.PublicExponent = OpenSSL.Core.BigNumber.FromArray(publicKey.yExponent);

            this.publicKey = publicKey;
        }
    }
}
