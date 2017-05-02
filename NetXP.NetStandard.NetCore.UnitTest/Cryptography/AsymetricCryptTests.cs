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
        public IContainer iuc { get; private set; }

        [TestInitialize]
        public void Init()
        {
            var container = new StructureMap.Container();
            container.Configure(cnf =>
            {
                SMRegisterExpression smre = new SMRegisterExpression(cnf);
                CompositionRoot.Init(smre);
                NetCore.CompositionRoot.Init(smre);

                iuc = new SMContainer(container);
                smre.RegisterInstance<IContainer, SMContainer>(iuc, LifeTime.Trasient);
            });

        }

        [TestMethod()]
        public void NC_IAsymetricCrypt_GenerateKeys()
        {
            var asymetricCrypt = this.iuc.Resolve<IAsymetricCrypt>();
            asymetricCrypt.GenerateKeys();
        }

        [TestMethod()]
        public void NC_IAsymetricCryptWithMSRSA_Decrypt()
        {
            var asymetricCrypt = this.iuc.Resolve<IAsymetricCrypt>();
            asymetricCrypt.GenerateKeys();

            string message = "Hola";
            var encryptedMessage = asymetricCrypt.Encrypt(Encoding.ASCII.GetBytes(message));

            var decryptedMessage = asymetricCrypt.Decrypt(encryptedMessage);

            var resultMessage = Encoding.ASCII.GetString(decryptedMessage);

            Assert.AreEqual(message, resultMessage);
        }

        [TestMethod()]
        public void NC_IAsymetricCryptBynarySerialize()
        {
            var asymetricCrypt = this.iuc.Resolve<IAsymetricCrypt>();
            var serializeTFactory = this.iuc.Resolve<IFactorySerializer>();

            asymetricCrypt.GenerateKeys();

            var jsonSerliazer = serializeTFactory.Resolve(SerializerType.Json);
            var privateKeySerialized = jsonSerliazer.Serialize(asymetricCrypt.GetPrivateKey());
            var publicKeySerialized = jsonSerliazer.Serialize(asymetricCrypt.GetPublicKey());

            var privateKeyBase64 = Convert.ToBase64String(privateKeySerialized);
            var publicKeyBase64 = Convert.ToBase64String(publicKeySerialized);

            PrivateKey privateKey = jsonSerliazer.Deserialize<PrivateKey>(privateKeySerialized);

            asymetricCrypt = this.iuc.Resolve<IAsymetricCrypt>();
            asymetricCrypt.SetPrivateKey(privateKey);

            string message = "Test for serialized privatekey";
            var encryptedMessage = asymetricCrypt.Encrypt(Encoding.ASCII.GetBytes(message));
            var decryptedMessage = asymetricCrypt.Decrypt(encryptedMessage);
            var resultMessage = Encoding.ASCII.GetString(decryptedMessage);

            Assert.AreEqual(message, resultMessage);
        }
    }
}