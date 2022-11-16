using System;
using NetXP.Serialization;
using NetXP.Cryptography;
using NetXP.DateAndTime;
using NetXP.DependencyInjection;
using NetXP.Network.TCP;
using NetXP.Network.TCP.Implementations;
using NetXP.Network.SecureLittleProtocol.Implementations;
using Microsoft.Extensions.Options;
using NetXP.Network.LittleJsonProtocol;
using NetXP.Network.LittleJsonProtocol.Implementations;
using NetXP.Configuration.Implementations;
using Microsoft.Extensions.Configuration;
using System.IO;
using NetXP.Auditory;
using NetXP.Network.SecureLittleProtocol.Implementation;
using NetXP.Network.Email.Implementations;
using NetXP.Network.Email;
using NetXP.Network.Proxy.Implementations;

namespace NetXP.Network
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
            uc.Register<IClientConnector, SocketClientConnector>("normal", DILifeTime.Trasient);

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
                                                                                        ctor.WithParameter<IOptions<PersistenPrivateKeyConfiguration>>();
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
            uc.Register<ILJPMessageFactory, MessageExtractor_v2_0>("2.0", DILifeTime.Singleton);

            //MAIL 
            uc.Register<IMailSender, MailSender>();
        }
    }
}
