using NetXP.NetStandard.Network;
using System;
using System.Net.Sockets;
using NetXP.NetStandard.Serialization;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.DateAndTime;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard;
using NetXP.NetStandard.Compression.Implementations;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Network.TCP.Implementation;
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
using NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps;
using NetXP.NetStandard.Factories;

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

            //var loggerMock = new Mock<ILogger>();
            //uc.RegisterInstance<ILogger>(loggerMock.Object, LifeTime.Singleton);

            //cmpr
            uc.Register<ICompression, DeflateCompress>(LifeTime.Singleton);

            #region NET 
            //TCP
            uc.Register<IServerTCP, ServerTCP>("normal", LifeTime.Trasient);
            uc.Register<ITCPClient, SocketClient>("normal", LifeTime.Trasient);
            uc.Register<ITCPClient, SocketClient>("normal-socket",
                                     LifeTime.Trasient,
                                     (ctor) =>
                                     {
                                         ctor.WithParameter<System.Net.Sockets.Socket>();
                                     });


            uc.Register<IFactoryClientTCP, FactoryClientTCP>("normal", LifeTime.Singleton);

            //SLP
            uc.Register<ITCPClient, ClientSLJP>(LifeTime.Trasient);
            uc.Register<IServerTCP, ServerSLJP>(LifeTime.Trasient);
            uc.Register<IFactoryClientTCP, FactoryClientSLJP>(LifeTime.Singleton);
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
            //uc.Register<ISymetricCrypt, Symetric>(LifeTime.Singleton);

            //TODO: SSL Crypter
            //uc.Register<IAsymetricCrypt, AsymetricCryptWithOpenSSL>(LifeTime.Trasient);
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
