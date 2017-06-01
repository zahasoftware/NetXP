using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetXP.NetStandard.NetFramework.Cryptography;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps;
using NetXP.NetStandard.NetFramework;
using NetXP.NetStandard.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// NOTE: Rebuild project if fail
namespace NetXP.NetStandard.NetFramework.Cryptography.Tests
{

    [TestClass()]
    public class AsymetricCryptTests
    {
        public IContainer container { get; private set; }

        [TestInitialize]
        public void Init()
        {
            var smContainer = new StructureMap.Container();
            container = new SMContainer(smContainer);

            container.Configuration.Configure(smre =>
            {
                CompositionRoot.RegisterNetFramework(smre);
                smre.RegisterInstance<IContainer>(container, LifeTime.Trasient);
            });

        }

        [TestMethod()]
        public void NF_IAsymetricCrypt_GenerateKeys()
        {
            var iasymetricCrypt = this.container.Resolve<IAsymetricCrypt>();
            iasymetricCrypt.GenerateKeys();
        }

        [TestMethod()]
        public void NF_IAsymetricCryptWithOpenSSL_Decrypt()
        {
            var iasymetricCrypt = this.container.Resolve<IAsymetricCrypt>();
            iasymetricCrypt.GenerateKeys();


            string message = "Hola";
            var encryptedMessage = iasymetricCrypt.Encrypt(Encoding.ASCII.GetBytes(message));

            var decryptedMessage = iasymetricCrypt.Decrypt(encryptedMessage);

            var resultMessage = Encoding.ASCII.GetString(decryptedMessage);

            Assert.AreEqual(message, resultMessage);
        }

        [TestMethod()]
        public void NF_IAsymetricCryptBynarySerialize()
        {
            var asymetricCrypt = this.container.Resolve<IAsymetricCrypt>();
            var serializeTFactory = this.container.Resolve<IFactorySerializer>();

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