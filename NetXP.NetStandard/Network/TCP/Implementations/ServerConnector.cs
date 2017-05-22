using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.TCP.Implementations
{
    public class ServerConnector : IServerConnector
    {
        private TcpListener tcpListener;
        private readonly IClientConnectorFactory clientConnectorFactory;

        public ServerConnector(IClientConnectorFactory clientConnectorFactory)
        {
            this.clientConnectorFactory = clientConnectorFactory;
        }

        public void Listen(IPAddress ipAddress, int port)
        {
            tcpListener = new TcpListener(ipAddress, port);
            tcpListener.Start();
        }

        public async Task<IClientConnector> Accept()
        {
            try
            {
                var socket = await this.tcpListener.AcceptSocketAsync();
                IClientConnector tcpClient = this.clientConnectorFactory.Create(socket);

                return tcpClient;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
