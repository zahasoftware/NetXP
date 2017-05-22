using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.TCP
{
    public interface IClientConnector
    {
        int Receive(byte[] inputBuffer, int offset, int length);
        int Send(byte[] outputBuffer, int offset, int length);
        void Connect(IPAddress ipAddress, int port);
        void Disconnect();
        bool IsConnected { get; }
        string RemoteEndPoint { get; }
        string LocalEndPoint { get; }
    }
}
