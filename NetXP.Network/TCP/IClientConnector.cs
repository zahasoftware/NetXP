using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.TCP
{
    public interface IClientConnector: IDisposable
    {
        int Receive(byte[] inputBuffer, int offset, int length);
        int Send(byte[] outputBuffer, int offset, int length);
        void Connect(IPAddress ipAddress, int port);
        void Disconnect(bool dispose = false);
        bool IsConnected { get; }
        string RemoteEndPoint { get; }
        string LocalEndPoint { get; }
    }
}
