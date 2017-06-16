using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetXP.NetStandard;
using NetXP.NetStandard.Configuration.Implementations;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps;
using NetXP.NetStandard.Network;

namespace SmartSecurity.NetStandard.NetFramework.UnitTests.Configuration.Implementations
{
    [TestClass]
    public class OptionsInstanceTests
    {
        private SMContainer container;

        [TestInitialize]
        public void TestInitialize()
        {
            var smContainer = new StructureMap.Container();
            this.container = new SMContainer(smContainer);

            container.Configuration.Configure(smre =>
            {
                CompositionRoot.RegisterNetXPStandard(smre, container);
                smre.RegisterInstance<IContainer>(container, LifeTime.Trasient);
            });
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void NF_IOptions_AnyType_Resolve()
        {
            IOptions<SLJPOption> optionsInstance = this.CreateOptionsInstance();
            var slpOptions = optionsInstance.Value;

            Assert.AreNotEqual(null, slpOptions);
        }

        private IOptions<SLJPOption> CreateOptionsInstance()
        {
            return this.container.Resolve<IOptions<SLJPOption>>();
        }
    }
}