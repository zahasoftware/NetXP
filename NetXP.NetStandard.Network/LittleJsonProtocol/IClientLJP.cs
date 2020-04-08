using NetXP.NetStandard.Network.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    public interface IClientLJP : IDisposable
    {
        LJPResponse<T> ReceiveResponse<T>(bool throwExceptionWithNoData = true) where T : class;//Client Common

        /// <summary>
        /// Receive a response from server or client.
        /// </summary>
        /// <param name="tpeNamespace">namespace where search the type of response.</param>
        /// <returns>LJPResponse object, that contain the object</returns>
        LJPResponse ReceiveResponse(ICollection<Type> tpeNamespace, bool bThrowExceptionWithNoData = true);

        LJPResponse ReceiveResponse(Type tpeNamespace, bool bThrowExceptionWithNoData = true);

        /// <summary>
        /// Send call to remote host to execute a method in a remote host.
        /// </summary>
        /// <param name="SendCallParameter">Parameter that contain the Interface, Method and parameters to send.</param>
        void SendCall(LJPCall SendCallParameter);

        /// <summary>
        /// Called after call SendCall from remote host, to receive to wich interface, method execute.
        /// </summary>
        /// <returns>LJPCall that contain Interface, Method and parameters.</returns>
        LJPCallReceived ReceiveCall(params string[] servicesLayers);//Server 

        void SendResponse(object o);//Server

        void Connect(IPAddress address, int port);
        void Connect(string domain, int port);

        void Disconnect(bool dispose = true);

        IClientConnector ClientTCP { get; set; }

        /// <summary>
        /// Used to prevent close connection
        /// It doesn't do anything with internal implementation of this class, is just used for external codes.
        /// </summary>
        bool KeepAlive { get; set; }

        void SendException(NetXP.NetStandard.Network.LittleJsonProtocol.LJPException ex);
    }
}
