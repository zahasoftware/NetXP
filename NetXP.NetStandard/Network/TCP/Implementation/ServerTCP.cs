using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.TCP.Implementation
{
    public class ServerTCP : IServerTCP
    {
        private TcpListener tcpListener;
        private readonly IFactoryClientTCP oIFactoryClientTCP;

        public ServerTCP(IFactoryProducer factoryProducer)
        {
            this.oIFactoryClientTCP = factoryProducer.Create(NetworkFactory.TransmissionControlProtocol);
        }

        public void Listen(IPAddress ipAddress, int port)
        {
            tcpListener = new TcpListener(ipAddress, port);
            tcpListener.Start();
        }

        public async Task<ITCPClient> Accept()
        {
            try
            {
                var socket = await this.tcpListener.AcceptSocketAsync();
                ITCPClient tcpClient = this.oIFactoryClientTCP.Create(socket);

                return tcpClient;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
