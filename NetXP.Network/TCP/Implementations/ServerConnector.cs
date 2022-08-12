using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.TCP.Implementations
{
    public class ServerConnector : IServerConnector
    {
        private TcpListener tcpListener;
        private readonly IClientConnectorFactory clientConnectorFactory;

        public ServerConnector(IClientConnectorFactoryProducer clientConnectorFactoryProducer)
        {
            this.clientConnectorFactory = clientConnectorFactoryProducer.CreateClient(ConnectorFactory.TransmissionControlProtocol);
        }

        public void Listen(IPAddress ipAddress, int port)
        {
            tcpListener = new TcpListener(ipAddress, port);
            tcpListener.Start();
        }

        public async Task<IClientConnector> Accept()
        {
            var socket = await this.tcpListener.AcceptSocketAsync();

            IClientConnector tcpClient = this.clientConnectorFactory.Create(socket);
            return tcpClient;


        }
    }
}
