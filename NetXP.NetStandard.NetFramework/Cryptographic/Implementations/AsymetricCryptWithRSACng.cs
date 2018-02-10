﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NetXP.NetStandard.Cryptography;

namespace NetXP.NetStandard.NetFramework.Cryptography.Implementations
{

    /// <summary>
    /// https://stackoverflow.com/questions/41986995/implement-rsa-in-net-core 
    /// This implementation works fine in NetFramework
    /// </summary>
    public class AsymetricCryptWithRSACng : IAsymetricCrypt, IDisposable
    {
        private RSACng rsa;
        private static object rsaLock = new object();

        public AsymetricCryptWithRSACng()
        {
            var key = GetKeyForCng();
            rsa = new RSACng(key);
        }

        public byte[] Decrypt(byte[] encryptedBytes)
        {
            return rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
        }


        public byte[] Encrypt(byte[] decryptedBytes)
        {
            return rsa.Encrypt(decryptedBytes, RSAEncryptionPadding.Pkcs1);
        }

        public PrivateKey GetPrivateKey()
        {
            //rsa.VerifyData();
            //rsa.VerifyHash();
            rsa.Key.SetProperty(
                new CngProperty(
                     "Export Policy",
                     BitConverter.GetBytes((int)CngExportPolicies.AllowPlaintextExport),
                     CngPropertyOptions.Persist));

            var rsaParam = rsa.ExportParameters(true);
            if (privKey == null)
            {
                byte[] yQ = new byte[rsaParam.Q.Length];
                byte[] yP = new byte[rsaParam.P.Length];

                byte[] yInverseQ = new byte[rsaParam.InverseQ.Length];
                byte[] yDP = new byte[rsaParam.DP.Length];
                byte[] yDQ = new byte[rsaParam.DQ.Length];

                //Public Part Key
                byte[] yPm = new byte[rsaParam.Modulus.Length];
                byte[] yPe = new byte[rsaParam.Exponent.Length];
                byte[] yD = new byte[rsaParam.D.Length];

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

                rsaParam.Q.CopyTo(privKey.yQ, 0);
                rsaParam.P.CopyTo(privKey.yP, 0);
                rsaParam.InverseQ.CopyTo(privKey.yInverseQ, 0);
                rsaParam.DP.CopyTo(privKey.yDP, 0);
                rsaParam.DQ.CopyTo(privKey.yDQ, 0);
                rsaParam.Modulus.CopyTo(privKey.yModulus, 0);
                rsaParam.Exponent.CopyTo(privKey.yExponent, 0);
                rsaParam.D.CopyTo(privKey.yD, 0);
            }
            return privKey;
        }

        PrivateKey privKey;
        PublicKey publicKey;
        public PublicKey GetPublicKey()
        {

            var rsaParam = this.rsa.ExportParameters(false);
            if (publicKey == null)
            {
                byte[] yPm = new byte[rsaParam.Modulus.Length];
                byte[] yPe = new byte[rsaParam.Exponent.Length];

                publicKey = new PublicKey()
                {
                    yModulus = yPm,
                    yExponent = yPe,
                };


                rsaParam.Modulus.CopyTo(publicKey.yModulus, 0);
                rsaParam.Exponent.CopyTo(publicKey.yExponent, 0);
            }
            return publicKey;
        }

        void IAsymetricCrypt.GenerateKeys()
        {
            lock (rsaLock)
            {
                if (rsa != null)
                {
                    rsa.Dispose();
                }

                CngKey key = GetKeyForCng();

                rsa = new RSACng(key);
                rsa.KeySize = 2048;

                privKey = null;
                this.publicKey = null;
            }
        }

        private static CngKey GetKeyForCng()
        {
            var ckcp = new CngKeyCreationParameters
            {
                KeyUsage = CngKeyUsages.AllUsages,
                ExportPolicy = CngExportPolicies.AllowPlaintextExport | CngExportPolicies.AllowExport | CngExportPolicies.AllowArchiving | CngExportPolicies.AllowPlaintextArchiving,
            };
            ckcp.Parameters.Add(new CngProperty("Length", BitConverter.GetBytes(2048), CngPropertyOptions.None));
            var key = CngKey.Create(CngAlgorithm.Rsa, null, ckcp);
            return key;
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
            {
                throw new Exception("Invalid Private Key.");
            }

            RSAParameters rsaParameters = new RSAParameters()
            {
                Q = privateKey.yQ,
                P = privateKey.yP,
                InverseQ = privateKey.yInverseQ,
                DP = privateKey.yDP,
                D = privateKey.yD,
                DQ = privateKey.yDQ,
                Exponent = privateKey.yExponent,
                Modulus = privateKey.yModulus
            };
            rsa.ImportParameters(rsaParameters);

            this.privKey = privateKey;
        }

        public void SetPublicKey(PublicKey publicKey)
        {
            if (publicKey.yModulus == null || publicKey.yExponent == null)
            {
                throw new Exception("Invalid Public Key.");
            }

            //rsa.PublicModulus = OpenSSL.Core.BigNumber.FromArray(publicKey.yModulus);
            //rsa.PublicExponent = OpenSSL.Core.BigNumber.FromArray(publicKey.yExponent);

            RSAParameters rsaParam = new RSAParameters();
            rsaParam.Modulus = publicKey.yModulus;
            rsaParam.Exponent = publicKey.yExponent;
            rsa.ImportParameters(rsaParam);
            this.publicKey = publicKey;
        }
    }
}
