using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetXP.DependencyInjection;
using NetXP.Cryptography;
using di = NetXP.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NetXP.SystemInformation;
using NetXP.CompositionRoots;
using NetXP.DependencyInjection.Implementations.LamarDI;

namespace NetXP.SystemInformation.Tests
{
    [TestClass()]
    public class ServiceInformer_Tests
    {
        private di.IContainer container;

        public ISymetricCrypt ISymetric { get; private set; }
        public string ServiceName { get; private set; }
        public string ServicePath { get; private set; }

        [TestInitialize]
        public void Init()
        {

            container = new LamarContainer([]);
            container.Configuration.Configure((IRegister cnf) =>
            {
                cnf.RegisterAllNetXP();
            });

             // Make a file with name unversionSettings.json with the follow data:
            // {
            //      "ServiceManager": {
            //          "ServiceName": "DMC-Device-Debug",
            //          "ServicePath": "C:\Program Files (x86)\DMC-Debug\DMC.Device.WindowService.exe"
            //      }
            // }
            var confBuilder = new ConfigurationBuilder ()
                .AddJsonFile ("unversionSettings.json");
            var conf = confBuilder.Build ();

            //Extracting data
            this.ServiceName = conf.GetSection ("ServiceManager:ServiceName").Value;
            this.ServicePath = conf.GetSection ("ServiceManager:ServicePath").Value;
        }

        //[TestMethod]
        public void NS_GetServices()
        {
            var serviceInformer = container.Resolve<IServiceInformer>();
            var services = serviceInformer.GetServices();
        }

        //[TestMethod]
        public void NS_GetService()
        {
            var serviceInformer = container.Resolve<IServiceInformer>();
            var services = serviceInformer.GetService(ServiceName);
        }

    }
    
}