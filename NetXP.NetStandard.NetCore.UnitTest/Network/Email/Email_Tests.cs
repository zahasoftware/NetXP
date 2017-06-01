﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace NetXP.NetStandard.NetCore.Cryptography.Tests
{
    [TestClass()]
    public class Email_Tests
    {
        private di.IContainer container;

        public ISymetricCrypt ISymetric { get; private set; }

        [TestInitialize]
        public void Init()
        {
            var smContainer = new StructureMap.Container();
            container = new SMContainer(smContainer);

            container.Configuration.Configure(smre =>
            {
                CompositionRoot.RegisterNetXPCore(smre);
                NetCore.CompositionRoot.RegisterNetXPCore(smre);
                smre.RegisterInstance<di.IContainer>(container, LifeTime.Trasient);
            });

        }

        [TestMethod]
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

            throw new NotImplementedException("Comment this to test.");

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