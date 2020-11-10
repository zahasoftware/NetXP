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
        public static void RegisterNetwork(this IRegister uc)
        {
            Type serializerType = typeof(ISerializer);
            Type hashType = typeof(IHash);
            Type loggerType = typeof(ILogger);

            //TCP
            uc.Register<IServerConnector, ServerConnector>("normal", DILifeTime.Trasient);
            uc.Register<IClientConnectorFactory, ClientConnectorFactory>("normal", DILifeTime.Singleton);
            uc.Register<IClientConnector, SocketClientConnector>("normal", DILifeTime.Trasient, (ctor) => ctor.Empty());


            //Proxy 
            //Proxy Connector
            uc.Register<IClientConnectorFactory, ClientProxyConnectorFactory>("proxy", DILifeTime.Singleton);
            uc.Register<IClientConnector, ClientProxyConnector>("proxy", DILifeTime.Trasient);

            //SLP
            uc.Register<System.Net.Sockets.Socket, System.Net.Sockets.Socket>(DILifeTime.Trasient, ctor =>
            {
                ctor.InjectInstance(System.Net.Sockets.AddressFamily.InterNetwork);
                ctor.InjectInstance(System.Net.Sockets.SocketType.Stream);
                ctor.InjectInstance(System.Net.Sockets.ProtocolType.Tcp);
            });

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
