using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps;
using NetXP.NetStandard.Serialization;
using System;
using System.Text;
using NetXP.NetStandard.NetCore;
using StructureMap;

/// NOTE: Rebuild project if fail
namespace NetXP.NetStandard.NetFramework.Cryptography.Tests
{

    [TestClass()]
    public class AsymetricCryptTests
    {
        public DependencyInjection.IContainer container { get; private set; }

        [TestInitialize]
        public void Init()
        {
            Container smapContainer = new Container();

            container = new SMContainer(smapContainer);
            container.Configuration.Configure((IRegister cnf) =>
           {
               cnf.AddNetXPNetCoreRegisters(container);
           });
        }

        [TestMethod()]
        public void NC_IAsymetricCrypt_GenerateKeys()
        {
            var asymetricCrypt = this.container.Resolve<IAsymetricCrypt>();
            asymetricCrypt.GenerateKeys();
        }

        [TestMethod()]
        public void NC_IAsymetricCryptWithMSRSA_Decrypt()
        {
            var asymetricCrypt = this.container.Resolve<IAsymetricCrypt>();
            asymetricCrypt.GenerateKeys();

            string message = "Hi";
            var encryptedMessage = asymetricCrypt.Encrypt(Encoding.ASCII.GetBytes(message));

            var decryptedMessage = asymetricCrypt.Decrypt(encryptedMessage);

            var resultMessage = Encoding.ASCII.GetString(decryptedMessage);

            Assert.AreEqual(message, resultMessage);
        }

        [TestMethod()]
        public void NC_IAsymetricCryptBynarySerialize()
        {
            var asymetricCrypt = this.container.Resolve<IAsymetricCrypt>();
            var serializeTFactory = this.container.Resolve<ISerializerFactory>();

            asymetricCrypt.GenerateKeys();

            var jsonSerliazer = serializeTFactory.Resolve(SerializerType.Json);
            var privateKeySerialized = jsonSerliazer.Serialize(asymetricCrypt.GetPrivateKey());
            var publicKeySerialized = jsonSerliazer.Serialize(asymetricCrypt.GetPublicKey());

            var privateKeyBase64 = Convert.ToBase64String(privateKeySerialized);
            var publicKeyBase64 = Convert.ToBase64String(publicKeySerialized);

            PrivateKey privateKey = jsonSerliazer.Deserialize<PrivateKey>(privateKeySerialized);

            asymetricCrypt = this.container.Resolve<IAsymetricCrypt>();
            asymetricCrypt.SetPrivateKey(privateKey);

            string message = "Test for serialized privatekey";
            var encryptedMessage = asymetricCrypt.Encrypt(Encoding.ASCII.GetBytes(message));
            var decryptedMessage = asymetricCrypt.Decrypt(encryptedMessage);
            var resultMessage = Encoding.ASCII.GetString(decryptedMessage);

            Assert.AreEqual(message, resultMessage);
        }
    }
}