using Microsoft.Extensions.Options;
using NetXP.NetStandard.Auditory;
using NetXP.NetStandard.Network.TCP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace NetXP.NetStandard.Network.Proxy.Implementations
{
    public class ClientProxyConnector : IClientConnector
    {
        private readonly IClientConnector clientConnector;
        private ProxyOptions proxyOptions;
        private readonly ILogger logger;
        private byte[] buffer = new byte[1024];

        public ClientProxyConnector(
            IClientConnectorFactoryProducer clientConnectorFactoryProducer,
            IOptions<ProxyOptions> proxyOptions,
            ILogger logger
            )
        {
            this.clientConnector = clientConnectorFactoryProducer.CreateClient(ConnectorFactory.TransmissionControlProtocol)
                                        .Create();

            this.proxyOptions = proxyOptions.Value;
            this.logger = logger;
        }

        public bool IsConnected => this.clientConnector.IsConnected;

        public string RemoteEndPoint => this.clientConnector.RemoteEndPoint;

        public string LocalEndPoint => this.clientConnector.LocalEndPoint;

        public void Connect(IPAddress ipAddress, int port)
        {
            if (proxyOptions == null || string.IsNullOrEmpty(proxyOptions.Server?.Trim()))
            {
                ///Trying to read proxy from WebRequest
                try
                {
                    var proxyRequest = WebRequest.GetSystemWebProxy();
                    var requestedUri = new Uri($"http://{ipAddress.ToString()}");
                    var proxyUri = proxyRequest.GetProxy(requestedUri);

                    if (proxyUri == requestedUri)
                    {
                        throw new ProxyNotFoundException();
                    }

                    proxyOptions = new ProxyOptions
                    {
                        Port = proxyUri.Port,
                        Server = Dns.GetHostAddresses(proxyUri.Host)[0].ToString()
                    };
                }
                catch (ProxyNotFoundException)
                {
                    proxyOptions = null;
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex);
                    proxyOptions = null;
                }
            }

            if (proxyOptions != null)///Proxy found
            {
                /// Basic of Proxy Protocol
                /// CONNECT: https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/CONNECT
                clientConnector.Connect(IPAddress.Parse(proxyOptions.Server), proxyOptions.Port);

                ///Proxy Handshake Connection
                var message = Encoding.ASCII.GetBytes($"CONNECT {ipAddress.ToString()}:{port} HTTP/1.1\r\n"
                                                    + $"Connection: keep-alive\r\n\r\n");
                clientConnector.Send(message, 0, message.Length);

                ///Receiving Proxy Response
                Array.Clear(buffer, 0, buffer.Length);
                clientConnector.Receive(buffer, 0, 1024);

                var bufferAsString = Encoding.ASCII.GetString(buffer);
                bufferAsString = bufferAsString.Replace('\0', ' ').Trim();

                if (!Regex.IsMatch(bufferAsString, @"^HTTP.+200\s+Connection\s+established$"))
                {
                    throw new ProxyConnectionException($"Connection Failed:{bufferAsString}");
                }
            }
            else///Direct connection (PROXY NOT FOUND)
            {
                clientConnector.Connect(ipAddress, port);
            }
        }

        public void Disconnect(bool dispose = true)
        {
            this.clientConnector.Disconnect();
        }

        public int Receive(byte[] inputBuffer, int offset, int length)
        {
            return this.clientConnector.Receive(inputBuffer, offset, length);
        }

        public int Send(byte[] outputBuffer, int offset, int length)
        {
            return this.clientConnector.Send(outputBuffer, offset, length);
        }
    }
}
