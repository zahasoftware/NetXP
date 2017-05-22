using NetXP.NetStandard.Network;
using System;
using NetXP.NetStandard.Serialization;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.DateAndTime;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Compression.Implementations;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Network.TCP.Implementations;
using NetXP.NetStandard.Network.SecureLittleProtocol.Implementation;
using Microsoft.Extensions.Options;
using NetXP.NetStandard.Network.LittleJsonProtocol;
using NetXP.NetStandard.Network.LittleJsonProtocol.Implementation;
using NetXP.NetStandard.Cryptography.Implementations;
using NetXP.NetStandard.Reflection;
using NetXP.NetStandard.Reflection.Implementations;
using NetXP.NetStandard.DateAndTime.Implementation;
using NetXP.NetStandard.Compression;
using NetXP.NetStandard.Serialization.Implementations;
using NetXP.NetStandard.Factories;
using NetXP.NetStandard.Configuration;
using NetXP.NetStandard.Configuration.Implementations;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace NetXP.NetStandard
{
    public static class CompositionRoot
    {
        public static void Init(IRegister uc)
        {
            Type serializerType = typeof(ISerializer);
            Type hashType = typeof(IHash);
            Type loggerType = typeof(ILogger);

            //cnf
            //oIUC.RegisterType<cnf.ISecureRemoteSDMConf, helper.cnf.i.SecureRemoteSDMConf>(new ContainerControlledLifetimeManager());
            //uc.RegisterType<IConfig, Configuration.i.SecureAppConfig>(ConfigType.AppConfigSecurity.ToString(), new ContainerControlledLifetimeManager());
            //uc.RegisterType<IConfig, cnf.i.SecureWebConfig>(ConfigType.WebConfigSecurity.ToString(), new ContainerControlledLifetimeManager());
            //c.Register<IConfig, ConfigDefault>(new ContainerControlledLifetimeManager());
            uc.Register<IConfigFactory, ConfigFactory>();

            //var loggerMock = new Mock<ILogger>();
            //uc.RegisterInstance<ILogger>(loggerMock.Object, LifeTime.Singleton);

            //cmpr
            uc.Register<ICompression, DeflateCompress>(LifeTime.Singleton);

            #region Network
            //TCP
            uc.Register<IServerConnector, ServerConnector>("normal", LifeTime.Trasient);
            uc.Register<IClientConnector, SocketClientConnector>("normal", LifeTime.Trasient);
            uc.Register<IClientConnector, SocketClientConnector>("normal-socket",
                                     LifeTime.Trasient,
                                     (ctor) =>
                                     {
                                         ctor.WithParameter<System.Net.Sockets.Socket>();
                                     });
            uc.Register<IClientConnectorFactory, ClientConnectorFactory>("normal", LifeTime.Singleton);

            //SLP
            uc.Register<IClientConnector, SLPClientConnector>(LifeTime.Trasient);
            uc.Register<IServerConnector, SLPServerConnector>(LifeTime.Trasient);
            uc.Register<IClientConnectorFactory, SLPClientConnectorFactory>(LifeTime.Singleton);
            uc.Register<IPersistentPrivateKeyProvider, PersistentPrivateKeyProvider>(LifeTime.Singleton,
                                                                                    (ctor) =>
                                                                                    {
                                                                                        ctor.WithParameter<ISerializer>();
                                                                                        ctor.WithParameter<IHash>();
                                                                                        ctor.WithParameter<ILogger>();
                                                                                        ctor.WithParameter<ICustomDateTime>();
                                                                                        ctor.WithParameter<IOptions<TCPOption>>();
                                                                                        ctor.InjectInstance(string.Empty);
                                                                                    });

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            var slpOptions = new SLJPOption();
            config.GetSection("SLP").Get<SLJPOption>();

            uc.RegisterInstance<IOptions<SLJPOption>, OptionsInstance<SLJPOption>>(new OptionsInstance<SLJPOption>(slpOptions), LifeTime.Singleton);

            //LJP
            uc.Register<IServerLJP, ServerLJP>(LifeTime.Trasient);
            uc.Register<IClientLJP, ClientLJP>(LifeTime.Trasient);
            uc.Register<IFactoryClientLJP, FactoryClientLJP>(LifeTime.Singleton);

            uc.Register<ILJPMessageFactory, MessageExtractor_v0_0>("0.0", LifeTime.Singleton);
            uc.Register<ILJPMessageFactory, MessageExtractor_v1_0>("1.0", LifeTime.Singleton);

            //MAIL 
            //TODO: Mail Sender
            //uc.Register<IMailSender, net.mail.i.MailSender>();

            #endregion

            //Reflect
            uc.Register<IReflector, Reflector>(LifeTime.Singleton);

            //Serializer
            uc.Register<IFactorySerializer, SerializeTFactory>(LifeTime.Singleton);
            uc.Register<ISerializer, Serialize2Json>(SerializerType.Json.ToString(), LifeTime.Singleton);
            uc.Register<ISerializer, Serialize2Xml>(SerializerType.Xml.ToString(), LifeTime.Singleton);

            //crypt
            uc.Register<INameResolverFactory<IAsymetricCrypt>, AsymetricFactory>(LifeTime.Singleton);
            uc.Register<IHash, HashMD5>(HashType.MD5.ToString(), LifeTime.Trasient);
            uc.Register<IHash, HashSHA256>(LifeTime.Trasient);
            uc.Register<IHashFactory, HashFactory>(LifeTime.Singleton);

            //sys
            //ISysInfo and IStorageInfo need to be implemented in their os system.
            //uc.Register<sys.ISysInfo, sys.i.SysInfo>(LifeTime.Singleton);
            //uc.Register<sys.IStorageInfo, sys.i.SysInfo>(LifeTime.Singleton);

            var customDateTime = new CustomDateTime(0);
            uc.RegisterInstance<ICustomDateTime, CustomDateTime>(customDateTime, LifeTime.Singleton);
        }
    }
}
