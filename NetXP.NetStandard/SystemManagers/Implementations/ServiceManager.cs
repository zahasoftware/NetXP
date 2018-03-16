using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetXP.NetStandard.Processes;
using NetXP.NetStandard.SystemInformation;

namespace NetXP.NetStandard.SystemManagers.Implementations
{
    public class ServiceManager : IServiceManager
    {
        private readonly IIOTerminal terminal;
        private readonly ISystemInformation systemInformation;
        private readonly IServiceInformer serviceInformer;
        private readonly IServiceManagerFactory serviceManagerFactory;

        public ServiceManager(
            IIOTerminal terminal,
            ISystemInformation systemInformation,
            IServiceInformer serviceInformer,
            IServiceManagerFactory serviceManagerFactory
        )
        {
            this.terminal = terminal;
            this.systemInformation = systemInformation;
            this.serviceInformer = serviceInformer;
            this.serviceManagerFactory = serviceManagerFactory;
        }

        public void Create(string serviceName, string binPath, ServiceCreateOptions serviceCreateOptions)
        {
            serviceManagerFactory
                .Create(systemInformation.GetOSInfo().Platform)
                .Create(serviceName, binPath, serviceCreateOptions);
        }

        public void Delete(string serviceName)
        {
            serviceManagerFactory
                .Create(systemInformation.GetOSInfo().Platform)
                .Delete(serviceName);
        }

        public void Start(string serviceName)
        {
            serviceManagerFactory
                .Create(systemInformation.GetOSInfo().Platform)
                .Start(serviceName);
        }

        public void Stop(string serviceName)
        {
            serviceManagerFactory
                .Create(systemInformation.GetOSInfo().Platform)
                .Stop(serviceName);
        }

        public void Uninstall(string serviceName)
        {
            serviceManagerFactory
                .Create(systemInformation.GetOSInfo().Platform)
                .Uninstall(serviceName);

        }
    }
}