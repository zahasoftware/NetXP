using NetXP.NetStandard.Exceptions;
using System.Net.Sockets;

namespace NetXP.NetStandard.Network.TCP.Implementations
{
    public class SocketClientConnector : IClientConnector
    {
        private System.Net.Sockets.Socket _oSocket;
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
            this.oSocket = socket;
            this.oSocket.ReceiveTimeout = tcpOptions.ReceiveTimeOut;
        }

        public SocketClientConnector(TCPOption tcpOptions, Socket socket) : this(socket)
        {
            this.tcpOptions = tcpOptions;
        }

        public void Connect(System.Net.IPAddress oIPAddress, int port)
        {
            this.oSocket = new System.Net.Sockets.Socket(
                  oIPAddress.AddressFamily
                , System.Net.Sockets.SocketType.Stream
                , System.Net.Sockets.ProtocolType.Tcp);
            this.oSocket.ReceiveTimeout = this.tcpOptions.ReceiveTimeOut;
            this.oSocket.Connect(oIPAddress, port);
        }

        public void Disconnect(bool dispose = true)
        {
            if (!this.Disposed)
            {
                if (this.oSocket.Connected)
                {
                    try
                    {
                        this.oSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                    }
                    finally
                    {
                        try
                        {
                            if (dispose)
                            {
                                this.oSocket.Dispose();
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

        public int Receive(byte[] aInputBuffer, int iOffset, int iLength)
        {
            return this.oSocket.Receive(aInputBuffer, iOffset, iLength, System.Net.Sockets.SocketFlags.None);
        }

        public int Send(byte[] aOutputBuffer, int iOffset, int iLength)
        {
            return this.oSocket.Send(aOutputBuffer, iOffset, iLength, System.Net.Sockets.SocketFlags.None);
        }

        public System.Net.Sockets.Socket oSocket
        {
            get
            {
                return this._oSocket;
            }
            set
            {
                if (value == null)
                    throw new CustomApplicationException("IClientTCP Socket can't be null");
                else
                    this._oSocket = value;
            }
        }

        public bool IsConnected
        {
            get
            {
                return !this.Disposed && this.oSocket.Connected;
            }
        }

        public string RemoteEndPoint
        {
            get
            {
                return this.Disposed ? $"[{nameof(this.Disposed)}]" : this.oSocket?.RemoteEndPoint?.ToString();
            }
        }

        public string LocalEndPoint
        {
            get
            {
                return this.Disposed ? $"[{nameof(this.Disposed)}]" : this.oSocket?.LocalEndPoint?.ToString();
            }
        }

        public bool Disposed { get; private set; } = false;
    }
}
