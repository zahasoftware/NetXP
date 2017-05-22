using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.TCP.Implementations
{
    public class TCPClientConnector : IClientConnector
    {
        private readonly TCPOption tcpOptions;
        private TcpClient tcpClient;
        private Stream stream;

        public TCPClientConnector()
        {
            this.tcpOptions = new TCPOption();
        }

        public TCPClientConnector(Socket socket)
        {
            if (this.tcpOptions == null)
            {
                this.tcpOptions = new TCPOption();
            }
            this.tcpClient = new TcpClient(socket.AddressFamily);
            this.tcpClient.Client = socket;
            this.stream = this.tcpClient.GetStream();
            this.stream.ReadTimeout = tcpOptions.ReceiveTimeOut;
        }

        public TCPClientConnector(TCPOption tcpOptions, Socket socket) : this(socket)
        {
            this.tcpOptions = tcpOptions;
        }


        public void Connect(System.Net.IPAddress oIPAddress, int iPort)
        {
            this.tcpClient = new TcpClient(oIPAddress.AddressFamily);
            tcpClient.ConnectAsync(oIPAddress, iPort).RunSynchronously();
            this.stream = this.tcpClient.GetStream();
            stream.ReadTimeout = this.tcpOptions.ReceiveTimeOut;
        }

        public void Disconnect()
        {
            if (tcpClient.Connected)
            {
                try { this.stream.Dispose(); }
                finally
                {
                    try { this.tcpClient.Dispose(); }
                    finally { }
                }
            }
        }

        public int Receive(byte[] aInputBuffer, int iOffset, int iLength)
        {
            return stream.Read(aInputBuffer, iOffset, iLength);
        }

        public int Send(byte[] aOutputBuffer, int iOffset, int iLength)
        {
            stream.Write(aOutputBuffer, iOffset, iLength);
            return iLength;
        }


        public bool IsConnected
        {
            get
            {
                return this.tcpClient.Connected;
            }
        }

        public string RemoteEndPoint
        {
            get
            {
                return this.tcpClient.Client.RemoteEndPoint.ToString();
            }
        }

        public string LocalEndPoint
        {
            get
            {
                return this.tcpClient.Client.LocalEndPoint.ToString();
            }
        }


    }
}
