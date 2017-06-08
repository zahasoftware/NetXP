using NetXP.NetStandard;
using NetXP.NetStandard.Auditory;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.Factories;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.SecureLittleProtocol.Implementations
{
    public class SLPServerConnector : IServerConnector
    {
        const short LITTLE_BUFFER_SIZE = 1024;
        const short MIDLE_BUFFER_SIZE = 1024 * 2;
        const short BIG_BUFFER_SIZE = 1024 * 4;
        const short PUBLIC_SIZE = 4;

        //To Reduce New
        private byte[] aLittleBuffer = new byte[LITTLE_BUFFER_SIZE];
        private byte[] aMidleBuffer = new byte[MIDLE_BUFFER_SIZE];
        private byte[] aBigBuffer = new byte[BIG_BUFFER_SIZE];

        private readonly IClientConnectorFactoryProducer clientConnectorFactoryProducer;
        private readonly ISerializer serialize;
        private readonly IServerConnector tcpServer;
        private readonly ILogger logger;
        private readonly INameResolverFactory<IAsymetricCrypt> asymetricCryptFactory;

        public IHash hash { get; private set; }

        public SLPServerConnector(
            IClientConnectorFactoryProducer clientConnectorFactoryProducer,
            INameResolverFactory<IAsymetricCrypt> asymetricCryptFactory,
            ISerializer serializer,
            IServerConnectorFactory serverFactory,
            ILogger ILogger,
            IHash IHash
            )
        {
            this.clientConnectorFactoryProducer = clientConnectorFactoryProducer;
            this.asymetricCryptFactory = asymetricCryptFactory;
            this.serialize = serializer;
            this.tcpServer = serverFactory.Create(ConnectorFactory.TransmissionControlProtocol); //Plain Text
            this.logger = ILogger;
            this.hash = IHash;
        }

        public void Listen(IPAddress oIPAddress, int iPort)
        {
            this.tcpServer.Listen(oIPAddress, iPort);
        }

        public async Task<IClientConnector> Accept()
        {
            var socket = await tcpServer.Accept();
            IClientConnector secureClient = clientConnectorFactoryProducer
                                                .CreateClient(ConnectorFactory.SecureLitleProtocol)
                                                .Create(socket);

            //Receive PublicKey And Send Their PublicKey
            secureClient.Receive(aLittleBuffer, 0, aLittleBuffer.Length);

            return secureClient;
        }
    }
}
