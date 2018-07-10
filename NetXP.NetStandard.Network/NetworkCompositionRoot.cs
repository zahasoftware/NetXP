using System;
using NetXP.NetStandard.Serialization;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.DateAndTime;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Network.TCP.Implementations;
using NetXP.NetStandard.Network.SecureLittleProtocol.Implementations;
using Microsoft.Extensions.Options;
using NetXP.NetStandard.Network.LittleJsonProtocol;
using NetXP.NetStandard.Network.LittleJsonProtocol.Implementations;
using NetXP.NetStandard.Configuration.Implementations;
using Microsoft.Extensions.Configuration;
using System.IO;
using NetXP.NetStandard.Auditory;
using NetXP.NetStandard.Network.SecureLittleProtocol.Implementation;
using NetXP.NetStandard.Network.Email.Implementations;
using NetXP.NetStandard.Network.Email;
using NetXP.NetStandard.Network.Proxy.Implementations;

namespace NetXP.NetStandard.Network
{
    public static class NetworkCompositionRoot
    {
        public static void RegisterNetwork(this IRegister uc, IContainer container, string appSettingFile = null)
        {
            Type serializerType = typeof(ISerializer);
            Type hashType = typeof(IHash);
            Type loggerType = typeof(ILogger);

            IConfigurationRoot config = null;
            if (
                   File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
                || (!string.IsNullOrEmpty(appSettingFile?.Trim()) && File.Exists(Path.Combine(Directory.GetCurrentDirectory(), appSettingFile)))
            )
            {
                config = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile(appSettingFile ?? "appsettings.json")
                                    .Build();
            }

            //TCP
            uc.Register<IServerConnector, ServerConnector>("normal", DILifeTime.Trasient);
            uc.Register<IClientConnectorFactory, ClientConnectorFactory>("normal", DILifeTime.Singleton);
            uc.Register<IClientConnector, SocketClientConnector>("normal", DILifeTime.Trasient, (ctor) => ctor.Empty());


            //Proxy 
            ///Configuration
            var proxyOptions = new ProxyOptions();
            config?.GetSection("Proxy")?.Bind(proxyOptions);
            uc.RegisterInstance<IOptions<ProxyOptions>>(new OptionsInstance<ProxyOptions>(proxyOptions), DILifeTime.Singleton);
            ///Proxy Connector
            uc.Register<IClientConnectorFactory, ClientProxyConnectorFactory>("proxy", DILifeTime.Singleton);
            uc.Register<IClientConnector, ClientProxyConnector>("proxy", DILifeTime.Trasient);

            //SLP
            uc.Register<System.Net.Sockets.Socket, System.Net.Sockets.Socket>(DILifeTime.Trasient, ctor => ctor.Empty());
            //uc.Register<IClientConnector, SocketClientConnector>("normal-socket",
            //                         LifeTime.Trasient, (ctor) => ctor.WithParameter<System.Net.Sockets.Socket>());
            //uc.Register<IClientConnector, SLPClientConnector>(LifeTime.Trasient);
            uc.Register<IServerConnector, SLPServerConnector>(DILifeTime.Trasient);
            uc.Register<IClientConnectorFactory, SLPClientConnectorFactory>(DILifeTime.Singleton);
            uc.Register<IPersistentPrivateKeyProvider, PersistentPrivateKeyProvider>(DILifeTime.Singleton,
                                                                                    (ctor) =>
                                                                                    {
                                                                                        ctor.WithParameter<ISerializer>();
                                                                                        ctor.WithParameter<IHash>();
                                                                                        ctor.WithParameter<ILogger>();
                                                                                        ctor.WithParameter<ICustomDateTime>();
                                                                                        ctor.WithParameter<IOptions<TCPOption>>();
                                                                                        ctor.InjectInstance(string.Empty);
                                                                                    });


            var slpOptions = new SLJPOption();
            config?.GetSection("SLP")?.Bind(slpOptions);
            uc.RegisterInstance<IOptions<SLJPOption>>(new OptionsInstance<SLJPOption>(slpOptions), DILifeTime.Singleton);

            var persistenPrivateKeyConfiguration = new PersistenPrivateKeyConfiguration();
            config?.GetSection("PPKConf")?.Bind(persistenPrivateKeyConfiguration);
            uc.RegisterInstance<IOptions<PersistenPrivateKeyConfiguration>>(
                new OptionsInstance<PersistenPrivateKeyConfiguration>(persistenPrivateKeyConfiguration), DILifeTime.Singleton);


            //SLP And TCP
            uc.Register<IClientConnectorFactoryProducer, ClientConnectorFactoryProducer>(DILifeTime.Singleton);
            uc.Register<IServerConnectorFactory, ServerConnectorFactory>(DILifeTime.Singleton);

            //LJP
            uc.Register<IServerLJP, ServerLJP>(DILifeTime.Trasient);
            uc.Register<IClientLJP, ClientLJP>(DILifeTime.Trasient);
            uc.Register<IFactoryClientLJP, FactoryClientLJP>(DILifeTime.Singleton);

            uc.Register<ILJPMessageFactory, MessageExtractor_v0_0>("0.0", DILifeTime.Singleton);
            uc.Register<ILJPMessageFactory, MessageExtractor_v1_0>("1.0", DILifeTime.Singleton);

            //MAIL 
            uc.Register<IMailSender, MailSender>();
        }
    }
}
