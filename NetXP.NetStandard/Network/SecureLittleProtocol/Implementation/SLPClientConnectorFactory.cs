﻿using Microsoft.Extensions.Options;
using NetXP.NetStandard.Compression;
using NetXP.NetStandard.Configuration;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.Cryptography.Implementations;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Factories;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.SecureLittleProtocol.Implementation
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

                return new SLPClientConnector(
                    container.Resolve<IClientConnectorFactoryProducer>(),
                    container.Resolve<INameResolverFactory<IAsymetricCrypt>>(),
                    container.Resolve<ISymetricCrypt>(),
                    container.Resolve<ISerializer>(),
                    container.Resolve<ILogger>(),
                    container.Resolve<IHash>(),
                    container.Resolve<IPersistentPrivateKeyProvider>(),
                    container.Resolve<ICompression>(),
                    container.Resolve<ISecureProtocolHandshake>(),
                    container.Resolve<IClientConnector>(),
                    container.Resolve<IOptions<SLJPOption>>()
                );
            }
            else
            {
                return this.container.Resolve<IClientConnector>();
            }
        }
    }
}
