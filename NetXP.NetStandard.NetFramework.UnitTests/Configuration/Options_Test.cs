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
using Microsoft.Extensions.Options;
using NetXP.NetStandard.NetFramework.UnitTests.Configuration;
using System.IO;
using Microsoft.Extensions.Configuration;
using NetXP.NetStandard.Serialization;

namespace NetXP.NetStandard.NetFramework.Configuration.Tests
{
    [TestClass()]
    public class Options_Tests
    {
        private di.IContainer c;
        public ISymetricCrypt symetric { get; private set; }

        [TestInitialize]
        public void Init()
        {
            var container = new Container();
            container.Configure(cnf =>
            {
                SMRegisterExpression smre = new SMRegisterExpression(cnf);
                CompositionRoot.Init(smre);

                smre.Register<IOptions<AppSettingWithOptions>, JsonOptions<AppSettingWithOptions>>();
                c = new SMContainer(container);
                smre.RegisterInstance<di.IContainer, SMContainer>(c, LifeTime.Trasient);
            });
        }

        [TestMethod()]
        public void NF_Options_GetValue()
        {
            var appCustom = this.c.Resolve<IOptions<AppSettingWithOptions>>();
            var asdf = appCustom.Value.MaxLength;

        }

        [TestMethod()]
        public void NF_IOptions_SaveValue()
        {
        }


    }
}