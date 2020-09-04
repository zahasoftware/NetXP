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
            this.tcpClient = new TcpClient();
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
            tcpClient.ConnectAsync(oIPAddress, iPort).RunSynchronously();
            this.stream = this.tcpClient.GetStream();
            stream.ReadTimeout = this.tcpOptions.ReceiveTimeOut;
        }

        public void Disconnect(bool dispose = false)
        {
            if (!this.Disposed && tcpClient.Connected)
            {
                try
                {
                    this.stream.Dispose();
                }
                finally
                {
                    try
                    {
                        if (dispose)
                        {
                            this.Dispose();
                            this.Disposed = true;
                        }
                    }
                    finally
                    {
                    }
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

        public void Dispose()
        {
            this.tcpClient?.Dispose();
            this.Disconnect();
        }

        public bool IsConnected
        {
            get
            {
                return !this.Disposed && this.tcpClient.Connected;
            }
        }

        public string RemoteEndPoint
        {
            get
            {
                try
                {
                    return this.Disposed ? $"[{nameof(this.Disposed)}]" : this.tcpClient.Client.RemoteEndPoint.ToString();
                }
                catch (Exception e)
                {
                    return $"[{e.Message}]";
                }
            }
        }

        public string LocalEndPoint
        {
            get
            {
                try
                {
                    return this.Disposed ? $"[{nameof(this.Disposed)}]" : this.tcpClient.Client.LocalEndPoint.ToString();
                }
                catch (Exception e)
                {
                    return $"[{e.Message}]";
                }
            }
        }

        public bool Disposed { get; private set; } = false;
    }
}
