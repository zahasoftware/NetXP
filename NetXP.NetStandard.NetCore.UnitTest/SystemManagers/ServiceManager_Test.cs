using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetXP.Cryptography;
using NetXP.DependencyInjection;
using NetXP.DependencyInjection.Implementations.StructureMaps;
using StructureMap;
using di = NetXP.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NetXP.Network.Email;
using NetXP.Processes;
using NetXP.SystemManagers;
using NetXP.CompositionRoots;

namespace NetXP.UnitTest.SystemManagers.Tests {
    [TestClass ()]
    public class ServiceManager_Tests {
        private di.IContainer container;

        public ISymetricCrypt ISymetric { get; private set; }
        /// <summary>
        /// Change service to an existing service for correct test.
        /// </summary>
        public string ServiceName;

        /// <summary>
        /// Service path where is service exe.
        /// </summary>
        public string ServicePath;

        [TestInitialize]
        public void Init () {
            Container smapContainer = new Container ();

            container = new SMContainer (smapContainer);
            container.Configuration.Configure ((IRegister cnf) => {
                cnf.RegisterAllNetXP ();
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

        [TestMethod]
        [ExpectedException (typeof (SystemManagerException))]
        public void NS_Delete_DeletingUnexistingService () {
            var serviceManager = container.Resolve<IServiceManager> ();
            serviceManager.Delete (ServiceName); //"Service Doesn't exist.");
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void NS_Start () {
            var serviceManager = container.Resolve<IServiceManager> ();
            serviceManager.Start (ServiceName);
        }

        [TestMethod]
        public void NS_Stop () {
            var serviceManager = container.Resolve<IServiceManager> ();
            serviceManager.Stop (ServiceName);
        }

        [TestMethod]
        public void NS_CreateService_WithDefaultOptions () {
            var serviceManager = container.Resolve<IServiceManager> ();
            serviceManager.Create (ServiceName, ServicePath);
        }

        [TestMethod]
        public void NS_Uninstall () {
            var serviceManager = container.Resolve<IServiceManager> ();
            serviceManager.Uninstall (ServiceName);
        }
    }
}