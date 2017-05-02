using NetXP.NetStandard;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.Factories;
using NetXP.NetStandard.Network.i;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.SecureLittleProtocol.Implementation
{
    public class ServerSLJP : IServerTCP
    {

        const short LITTLE_BUFFER_SIZE = 1024;
        const short MIDLE_BUFFER_SIZE = 1024 * 2;
        const short BIG_BUFFER_SIZE = 1024 * 4;
        const short PUBLIC_SIZE = 4;

        //To Reduce New
        private byte[] aLittleBuffer = new byte[LITTLE_BUFFER_SIZE];
        private byte[] aMidleBuffer = new byte[MIDLE_BUFFER_SIZE];
        private byte[] aBigBuffer = new byte[BIG_BUFFER_SIZE];

        private readonly IFactoryClientTCP oIFactoryClientTCP;
        private readonly ISerializer ISerializeT;
        private readonly IServerTCP IServerTCP;
        private readonly ILogger ILogger;
        private readonly INameResolverFactory<IAsymetricCrypt> IAsymetricCryptFactory;

        public IHash IHash { get; private set; }

        public ServerSLJP(
            IFactoryClientTCP oIFactoryClientTCP,
            INameResolverFactory<IAsymetricCrypt> IAsymetricCryptFactory,
            ISerializer ISerializeT,
            IFactoryProducer factoryProducer
            //[Dependency("normal")]IServerTCP IServerTCP
            , ILogger ILogger
            , IHash IHash
            )
        {
            this.oIFactoryClientTCP = oIFactoryClientTCP;
            this.IAsymetricCryptFactory = IAsymetricCryptFactory;
            this.ISerializeT = ISerializeT;
            this.IServerTCP = factoryProducer.CreateServer(NetworkFactory.TransmissionControlProtocol)
                                                           .Create(); //IServerTCP;
            this.ILogger = ILogger;
            this.IHash = IHash;
        }

        public void Listen(IPAddress oIPAddress, int iPort)
        {
            this.IServerTCP.Listen(oIPAddress, iPort);
        }

        public async Task<ITCPClient> Accept()
        {
            try
            {
                var socket = await IServerTCP.Accept();
                ITCPClient oIClientTCP = oIFactoryClientTCP.Create(socket);

                //Receive PublicKey And Send Their PublicKey
                oIClientTCP.Receive(aLittleBuffer, 0, aLittleBuffer.Length);

                return oIClientTCP;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
