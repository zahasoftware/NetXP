using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps;
using StructureMap;
using di = NetXP.NetStandard.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NetXP.NetStandard.SystemInformation;
using System.Net.NetworkInformation;
using System.Linq;

namespace NetXP.NetStandard.NetCore.SystemInformation.Tests
{
    [TestClass()]
    public class SystemInformer_Tests
    {
        private di.IContainer container;

        public ISymetricCrypt ISymetric { get; private set; }
        public string ServiceName { get; private set; }
        public string ServicePath { get; private set; }

        [TestInitialize]
        public void Init()
        {
            Container smapContainer = new Container();

            container = new SMContainer(smapContainer);
            container.Configuration.Configure((IRegister cnf) =>
            {
                cnf.RegisterAllNetXP(container);
            });

            // Make a file with name unversionSettings.json with the follow data:
            // {
            //      "ServiceManager": {
            //          "ServiceName": "DMC-Device-Debug",
            //          "ServicePath": "C:\Program Files (x86)\DMC-Debug\DMC.Device.WindowService.exe"
            //      }
            // }
            var confBuilder = new ConfigurationBuilder()
                .AddJsonFile("unversionSettings.json");
            var conf = confBuilder.Build();

            //Extracting data
            this.ServiceName = conf.GetSection("ServiceManager:ServiceName").Value;
            this.ServicePath = conf.GetSection("ServiceManager:ServicePath").Value;
        }

        [TestMethod]
        public void NS_GetOSInfo()
        {
            var systemInformer = container.Resolve<ISystemInformation>();
            var osInfo = systemInformer.GetOSInfo();
        }

      [TestMethod]
        public void NS_GetServices()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            var firstMacAddres = nics.FirstOrDefault().GetPhysicalAddress().ToString();
        }



    }

}