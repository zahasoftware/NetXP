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
using Microsoft.Extensions.Configuration;
using NetXP.NetStandard.Network.Email;
using NetXP.NetStandard.Processes;
using NetXP.NetStandard.SystemInformation;

namespace NetXP.NetStandard.NetCore.Cryptography.Tests
{
    [TestClass()]
    public class ServiceInformer_Tests
    {
        private di.IContainer container;

        public ISymetricCrypt ISymetric { get; private set; }

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

        [TestMethod]
        public void NC_GetServices()
        {
            var serviceInformer = container.Resolve<IServiceInformer>();
            var services = serviceInformer.GetServices();
        }

    }
    
}