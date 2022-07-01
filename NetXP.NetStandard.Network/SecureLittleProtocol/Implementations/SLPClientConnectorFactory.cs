﻿using Microsoft.Extensions.Options;
using NetXP.Auditory;
using NetXP.Compression;
using NetXP.Cryptography;
using NetXP.DependencyInjection;
using NetXP.Factories;
using NetXP.Network.TCP;
using NetXP.Serialization;
using System;
using System.Linq;

namespace NetXP.Network.SecureLittleProtocol.Implementations
{
    public class SLPClientConnectorFactory : IClientConnectorFactory
    {
        private IContainer container;
        public SLPClientConnectorFactory(IContainer container)
        {
            this.container = container;
        }

        public IClientConnector Create(params object[] @params)
        {
            if (@params.Count() > 0)
            {
                var tryTcpClient = @params.SingleOrDefault(o => o is IClientConnector);
                var tcpClient = tryTcpClient as IClientConnector;
                if (tcpClient == null)
                {
                    throw new ArgumentException("Parameter should be of IClientConnector type");
                }

                return new SLPClientConnector(
                    container.Resolve<INameResolverFactory<IAsymetricCrypt>>(),
                    container.Resolve<ISymetricCrypt>(),
                    container.Resolve<ISerializerFactory>(),
                    container.Resolve<ILogger>(),
                    container.Resolve<IHash>(),
                    container.Resolve<IPersistentPrivateKeyProvider>(),
                    container.Resolve<ICompression>(),
                    container.Resolve<ISecureProtocolHandshake>(),
                    tcpClient,
                    container.Resolve<IOptions<SLJPOption>>()
                );
            }
            else
            {
                return new SLPClientConnector(
                    container.Resolve<INameResolverFactory<IAsymetricCrypt>>(),
                    container.Resolve<ISymetricCrypt>(),
                    container.Resolve<ISerializerFactory>(),
                    container.Resolve<ILogger>(),
                    container.Resolve<IHash>(),
                    container.Resolve<IPersistentPrivateKeyProvider>(),
                    container.Resolve<ICompression>(),
                    container.Resolve<ISecureProtocolHandshake>(),
                    container.Resolve<IClientConnector>("proxy"),
                    container.Resolve<IOptions<SLJPOption>>()
                );
            }
        }
    }
}
