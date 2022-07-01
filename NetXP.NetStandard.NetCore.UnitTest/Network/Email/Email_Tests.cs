using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetXP.DependencyInjection;
using NetXP.Cryptography;
using NetXP.DependencyInjection.Implementations.StructureMaps;
using StructureMap;
using di = NetXP.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NetXP.Network.Email;
using NetXP.CompositionRoots;

namespace NetXP.UnitTest.Cryptography
{
    [TestClass()]
    public class Email_Tests
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
                cnf.RegisterAllNetXP();
            });
        }

        //[TestMethod]
        public void NC_IMailSender()
        {
            // Make a file with name unversionSettings.json with the follow data:
            //
            //      {
            //            "Email": {
            //                "Server": "your_mail_server",
            //                "Port": "your_server_port",
            //                "User": "your_username",
            //                "Password": "your_password",
            //                "ToEmail": "to_mail",
            //                "ToAlias": "to_alias",
            //                "FromEmail": "your_password",
            //                "FromAlias": "your_password"
            //            }
            //        }
            var confBuilder = new ConfigurationBuilder()
                .AddJsonFile("unversionSettings.json");
            var conf = confBuilder.Build();

            //Extracting data
            var server = conf.GetSection("Email:Server").Value;
            bool isValidPort = int.TryParse(conf.GetSection("Email:Port").Value, out int port);
            var user = conf.GetSection("Email:User").Value;
            var password = conf.GetSection("Email:Password").Value;
            var toMail = conf.GetSection("Email:ToEmail").Value;
            var toAlias = conf.GetSection("Email:ToAlias").Value;
            var fromMail = conf.GetSection("Email:FromEmail").Value;
            var fromAlias = conf.GetSection("Email:FromAlias").Value;

            var mailSender = this.container.Resolve<IMailSender>();
            mailSender.Server(new MailServer(server, port, user, password));

            var message = new MailMessage()
            {
                Body = "Body: Unit Test From Net Core.",
                Subject = "Subject: Unit Test From Net Core",
                IsBodyHtml = true,
                From = new MailAddress(fromMail, fromAlias)
            };
            message.To.Add(new MailAddress(toMail, toAlias));

            mailSender.Send(message);
        }

    }
}