using NetXP.NetStandard.Network;
using System;
using NetXP.NetStandard.Serialization;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.DateAndTime;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Compression.Implementations;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Network.TCP.Implementations;
using NetXP.NetStandard.Network.SecureLittleProtocol.Implementations;
using Microsoft.Extensions.Options;
using NetXP.NetStandard.Network.LittleJsonProtocol;
using NetXP.NetStandard.Network.LittleJsonProtocol.Implementations;
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
using NetXP.NetStandard.Auditory;
using NetXP.NetStandard.Network.SecureLittleProtocol.Implementation;
using NetXP.NetStandard.Network.Email.Implementations;
using NetXP.NetStandard.Network.Email;
using NetXP.NetStandard.Auditory.Implementations;

namespace NetXP.NetStandard
{
    public static class CompositionRoot
    {
        public static void RegisterNetXPStandard(this IRegister uc, IContainer container, string appSettingFile = null)
        {
            Type serializerType = typeof(ISerializer);
            Type hashType = typeof(IHash);
            Type loggerType = typeof(ILogger);

            //DI 
            uc.RegisterInstance(container, LifeTime.Singleton);

            //cnf
            //oIUC.RegisterType<cnf.ISecureRemoteSDMConf, helper.cnf.i.SecureRemoteSDMConf>(new ContainerControlledLifetimeManager());
            //uc.RegisterType<IConfig, Configuration.i.SecureAppConfig>(ConfigType.AppConfigSecurity.ToString(), new ContainerControlledLifetimeManager());
            //uc.RegisterType<IConfig, cnf.i.SecureWebConfig>(ConfigType.WebConfigSecurity.ToString(), new ContainerControlledLifetimeManager());
            //c.Register<IConfig, ConfigDefault>(new ContainerControlledLifetimeManager());
            uc.Register<IConfigFactory, ConfigFactory>();
            uc.Register<ILogger, Log4NetLogger>(LifeTime.Singleton);

            //cmpr
            uc.Register<ICompression, DeflateCompress>(LifeTime.Singleton);

            #region Network
            //TCP
            uc.Register<IServerConnector, ServerConnector>("normal", LifeTime.Trasient);
            uc.Register<IClientConnectorFactory, ClientConnectorFactory>("normal", LifeTime.Singleton);
            uc.Register<IServerConnectorFactory, ServerConnectorFactory>(LifeTime.Singleton);

            //SLP
            uc.Register<System.Net.Sockets.Socket, System.Net.Sockets.Socket>(LifeTime.Trasient, ctor => ctor.Empty());

            uc.Register<IClientConnector, SocketClientConnector>("normal", LifeTime.Trasient, (ctor) => ctor.Empty());
            //uc.Register<IClientConnector, SocketClientConnector>("normal-socket",
            //                         LifeTime.Trasient, (ctor) => ctor.WithParameter<System.Net.Sockets.Socket>());
            //uc.Register<IClientConnector, SLPClientConnector>(LifeTime.Trasient);

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

            IConfigurationRoot config = null;
            if (!string.IsNullOrEmpty(appSettingFile)
                && File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")))
            {

                config = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile(appSettingFile ?? "appsettings.json")
                                    .Build();
            }

            var slpOptions = new SLJPOption();
            config?.GetSection("SLP")?.Bind(slpOptions);
            uc.RegisterInstance<IOptions<SLJPOption>>(new OptionsInstance<SLJPOption>(slpOptions), LifeTime.Singleton);

            var persistenPrivateKeyConfiguration = new PersistenPrivateKeyConfiguration();
            config?.GetSection("PPKConf")?.Bind(slpOptions);
            uc.RegisterInstance<IOptions<PersistenPrivateKeyConfiguration>>(
                new OptionsInstance<PersistenPrivateKeyConfiguration>(persistenPrivateKeyConfiguration), LifeTime.Singleton);
            //SLP And TCP
            uc.Register<IClientConnectorFactoryProducer, ClientConnectorFactoryProducer>(LifeTime.Singleton);

            //LJP
            uc.Register<IServerLJP, ServerLJP>(LifeTime.Trasient);
            uc.Register<IClientLJP, ClientLJP>(LifeTime.Trasient);
            uc.Register<IFactoryClientLJP, FactoryClientLJP>(LifeTime.Singleton);

            uc.Register<ILJPMessageFactory, MessageExtractor_v0_0>("0.0", LifeTime.Singleton);
            uc.Register<ILJPMessageFactory, MessageExtractor_v1_0>("1.0", LifeTime.Singleton);

            //MAIL 
            uc.Register<IMailSender, MailSender>();

            #endregion

            //Reflect
            uc.Register<IReflector, Reflector>(LifeTime.Singleton);

            //Serializer
            uc.Register<ISerializerFactory, SerializeTFactory>(LifeTime.Singleton);
            uc.Register<ISerializer, Serialize2Xml>(SerializerType.Xml.ToString(), LifeTime.Singleton);
            uc.Register<ISerializer, Serialize2Json>(SerializerType.Json.ToString(), LifeTime.Singleton);

            #region Crypt
            uc.Register<INameResolverFactory<IAsymetricCrypt>, AsymetricFactory>(LifeTime.Singleton);
            uc.Register<IHash, HashMD5>(HashType.MD5.ToString(), LifeTime.Trasient);
            uc.Register<IHash, HashSHA256>(LifeTime.Trasient);
            uc.Register<IHashFactory, HashFactory>(LifeTime.Singleton);
            #endregion

            //sys
            //ISysInfo and IStorageInfo need to be implemented in their os system.
            //uc.Register<sys.ISysInfo, sys.i.SysInfo>(LifeTime.Singleton);
            //uc.Register<sys.IStorageInfo, sys.i.SysInfo>(LifeTime.Singleton);
            var customDateTime = new CustomDateTime(0);
            uc.RegisterInstance<ICustomDateTime>(customDateTime, LifeTime.Singleton);
        }
    }
}
