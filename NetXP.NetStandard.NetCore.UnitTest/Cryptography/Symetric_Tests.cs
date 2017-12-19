using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetXP.NetStandard.NetFramework.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps;
using StructureMap;
using di = NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.NetCore;

namespace NetXP.NetStandard.NetFramework.Cryptography.Tests
{
    [TestClass()]
    public class Symetric_Tests
    {
        private di.IContainer c;

        public ISymetricCrypt ISymetric { get; private set; }

        [TestInitialize]
        public void Init()
        {
            Container smapContainer = new Container();

            c = new SMContainer(smapContainer);
            c.Configuration.Configure((IRegister cnf) =>
            {
                cnf.AddNetXPNetCoreRegisters(c);
            });

            this.ISymetric = this.c.Resolve<ISymetricCrypt>();
        }

        [TestMethod()]
        public void NC_ISymetric_Encrypt_And_Decrypt()
        {
            SymetricKey SymetricKey = new SymetricKey();

            var aes = Aes.Create();
            SymetricKey.Key = aes.Key;
            SymetricKey.Salt = aes.IV;

            string sText = "Encriptando texto con llave aleatoria.";
            byte[] aText = Encoding.ASCII.GetBytes(sText);

            var aEncryptedText = this.ISymetric.Encrypt(aText, SymetricKey);
            var aUnencryptedText = this.ISymetric.Decrypt(aEncryptedText, SymetricKey);

            string sUnencryptedText = Encoding.ASCII.GetString(aUnencryptedText);

            Assert.AreEqual(sText, sUnencryptedText);
        }

        [TestMethod()]
        public void NC_ISymetric_Encrypt_And_Decrypt_WithLessOf16Bytes()
        {
            SymetricKey SymetricKey = new SymetricKey()
            {
                Key = Encoding.ASCII.GetBytes("MyPassword")
            };

            string sText = "Encriptando texto.";
            byte[] aText = Encoding.ASCII.GetBytes(sText);

            var aEncryptedText = this.ISymetric.Encrypt(aText, SymetricKey);
            var aUnencryptedText = this.ISymetric.Decrypt(aEncryptedText, SymetricKey);

            string sUnencryptedText = Encoding.ASCII.GetString(aUnencryptedText);

            Assert.AreEqual(sText, sUnencryptedText);
        }

        [TestMethod()]
        public void NC_ISymetric_Encrypt_And_Decrypt_WithGreaterThan16Bytes()
        {
            SymetricKey SymetricKey = new SymetricKey();
            SymetricKey.Key = Encoding.ASCII.GetBytes("MyPasswordMyPassword");

            string sText = "Encriptando texto.";
            byte[] aText = Encoding.ASCII.GetBytes(sText);

            var aEncryptedText = this.ISymetric.Encrypt(aText, SymetricKey);
            var aUnencryptedText = this.ISymetric.Decrypt(aEncryptedText, SymetricKey);

            string unencryptedText = Encoding.ASCII.GetString(aUnencryptedText);
            Assert.AreEqual(sText, unencryptedText);
        }
    }
}