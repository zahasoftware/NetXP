using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.TCP
{
    public interface ITCPClient
    {
        int Receive(byte[] aInputBuffer, int iOffset, int iLength);
        int Send(byte[] aOutputBuffer, int iOffset, int iLength);
        void Connect(IPAddress oIPAddress, int iPort);
        void Disconnect();
        bool IsConnected { get; }
        string RemoteEndPoint { get; }
        string LocalEndPoint { get; }
    }
}
