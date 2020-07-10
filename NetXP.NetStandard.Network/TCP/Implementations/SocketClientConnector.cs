using NetXP.NetStandard.Exceptions;
using System.Net.Sockets;

namespace NetXP.NetStandard.Network.TCP.Implementations
{
    public class SocketClientConnector : IClientConnector
    {
        private System.Net.Sockets.Socket socket;
        private readonly TCPOption tcpOptions;

        public SocketClientConnector()
        {
            this.tcpOptions = new TCPOption();
        }

        public SocketClientConnector(Socket socket)
        {
            if (this.tcpOptions == null)
            {
                this.tcpOptions = new TCPOption();
            }
            this.Socket = socket;
            this.Socket.ReceiveTimeout = tcpOptions.ReceiveTimeOut;
        }

        public SocketClientConnector(TCPOption tcpOptions, Socket socket) : this(socket)
        {
            this.tcpOptions = tcpOptions;
        }

        public void Connect(System.Net.IPAddress ipAddress, int port)
        {
            this.Socket = new System.Net.Sockets.Socket(
                  ipAddress.AddressFamily
                , System.Net.Sockets.SocketType.Stream
                , System.Net.Sockets.ProtocolType.Tcp);
            this.Socket.ReceiveTimeout = this.tcpOptions.ReceiveTimeOut;
            this.Socket.Connect(ipAddress, port);
        }

        public void Disconnect(bool dispose = true)
        {
            if (!this.Disposed)
            {
                if (this.Socket.Connected)
                {
                    try
                    {
                        this.Socket.Shutdown(SocketShutdown.Both);
                        this.Socket.Disconnect(false);
                    }
                    finally
                    {
                        try
                        {
                            if (dispose)
                            {
                                this.Socket.Dispose();
                                this.Disposed = true;
                            }
                        }
                        finally
                        {
                        }
                    }
                }
            }
        }

        public int Receive(byte[] inputBuffer, int offset, int length)
        {
            return this.Socket.Receive(inputBuffer, offset, length, System.Net.Sockets.SocketFlags.None);
        }

        public int Send(byte[] outputBuffer, int offset, int length)
        {
            return this.Socket.Send(outputBuffer, offset, length, System.Net.Sockets.SocketFlags.None);
        }

        public System.Net.Sockets.Socket Socket
        {
            get
            {
                return this.socket;
            }
            set
            {
                if (value == null)
                    throw new CustomApplicationException("IClientTCP Socket can't be null");
                else
                    this.socket = value;
            }
        }

        public bool IsConnected
        {
            get
            {
                return !this.Disposed && this.Socket.Connected;
            }
        }

        public string RemoteEndPoint
        {
            get
            {
                return this.Disposed ? $"[{nameof(this.Disposed)}]" : this.Socket?.RemoteEndPoint?.ToString();
            }
        }

        public string LocalEndPoint
        {
            get
            {
                return this.Disposed ? $"[{nameof(this.Disposed)}]" : this.Socket?.LocalEndPoint?.ToString();
            }
        }

        public bool Disposed { get; private set; } = false;
    }
}
