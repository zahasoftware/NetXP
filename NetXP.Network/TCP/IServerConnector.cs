using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.TCP
{
    public interface IServerConnector
    {

        Task<IClientConnector> Accept();
        /// 
        /// <param name="port"></param>
        void Listen(IPAddress ipAddress, int port);

    }//end IServerTCP

}//end namespace NetXP.NetStandard
