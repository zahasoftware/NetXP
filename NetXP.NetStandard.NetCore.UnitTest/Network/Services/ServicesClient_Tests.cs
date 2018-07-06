using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Network.Proxy.Implementations;
using System.Net;
using NetXP.NetStandard.Configuration.Implementations;
using Microsoft.Extensions.Options;
using NetXP.NetStandard.Network.Services;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace NetXP.NetStandard.NetCore.UnitTest.Network.Proxy
{
    [TestClass()]
    public class ServiceClient_Tests
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

        /// <summary>
        /// Download asmx service from here:
        /// https://developer.xamarin.com/samples/xamarin-forms/WebServices/TodoASMX/
        /// How to resolve problem "Target “build” does not exist in the project for Visual Studio" here:
        /// https://stackoverflow.com/a/2421944
        /// </summary>
        [TestMethod]
        public void NCNF_ServiceClient_ASMX()
        {
            var factory = container.Resolve<IServiceClientFactory>();

            var client = factory.Create(ServiceClientType.SoapV11);

            var response = client.Request<GetTodoItemsResponse>(
                new Uri("http://localhost:49178/TodoService.asmx"),
                "GetTodoItems",
                "http://www.xamarin.com/webservices/"
                ).Result;

            var count = response.TodoItems.Count;

            //Assert.AreNotEqual(0, count);

        }

        [DataContract(Name = "GetTodoItemsResponse", Namespace = "http://www.xamarin.com/webservices/")]
        public class GetTodoItemsResponse
        {
            [DataMember(Name = "GetTodoItemsResult")]
            public GetTodoItemsResult TodoItems { get; set; }
        }

        [CollectionDataContract(Name = "GetTodoItemsResult", ItemName = "TodoItem", Namespace = "http://www.xamarin.com/webservices/")]
        public class GetTodoItemsResult : List<TodoItem>
        { }


        [DataContract(Name = "TodoItem", Namespace = "http://www.xamarin.com/webservices/")]
        public class TodoItem
        {
            [DataMember(Name = "ID")]
            public string ID { get; set; }
            [DataMember(Name = "Name")]
            public string Name { get; set; }
            [DataMember(Name = "Notes")]
            public string Notes { get; set; }
            [DataMember(Name = "Done")]
            public bool Done { get; set; }
        }





    }
}