using System;
using NetXP.NetStandard.Serialization;
using NetXP.NetStandard.Cryptography;
using NetXP.NetStandard.DateAndTime;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Compression.Implementations;
using Microsoft.Extensions.Options;
using NetXP.NetStandard.Reflection;
using NetXP.NetStandard.Reflection.Implementations;
using NetXP.NetStandard.DateAndTime.Implementation;
using NetXP.NetStandard.Compression;
using NetXP.NetStandard.Configuration;
using NetXP.NetStandard.Configuration.Implementations;
using Microsoft.Extensions.Configuration;
using System.IO;
using NetXP.NetStandard.Auditory;
using NetXP.NetStandard.Processes.Implementations;
using NetXP.NetStandard.SystemInformation;
using NetXP.NetStandard.SystemInformation.Implementations;
using NetXP.NetStandard.SystemManagers;
using NetXP.NetStandard.SystemManagers.Implementations;

namespace NetXP.NetStandard
{
    public static class CompositionRoot
    {
        public static void RegisterNetXPStandard(this IRegister uc)
        {
            Type serializerType = typeof(ISerializer);
            Type hashType = typeof(IHash);
            Type loggerType = typeof(ILogger);

            //cnf
            uc.Register<IConfigFactory, ConfigFactory>();

            //cmpr
            uc.Register<ICompression, DeflateCompress>(DILifeTime.Singleton);


            #region Processes

            //Process
            uc.Register<NetStandard.Processes.IIOTerminal, IOTerminal>();

            #endregion

            //Reflect
            uc.Register<IReflector, Reflector>(DILifeTime.Singleton);

            //System Information
            //ISysInfo need to be implemented in their os system.
            uc.Register<NetStandard.SystemInformation.IStorageInfo, SystemInformation.Implementations.SysInfo>();
            var customDateTime = new CustomDateTime(0);
            uc.RegisterInstance<ICustomDateTime>(customDateTime, DILifeTime.Singleton);
            uc.Register<IServiceInformer, ServiceInformer>(DILifeTime.Singleton);

            //System Managers
            uc.Register<IServiceManager, ServiceManagerForWindows>(OSPlatformType.Windows.ToString(), DILifeTime.Singleton);
            uc.Register<IServiceManager, ServiceManagerForLinux>(OSPlatformType.Linux.ToString(), DILifeTime.Singleton);
            uc.Register<IServiceManager, ServiceManager>(DILifeTime.Singleton);
            uc.Register<IServiceManagerFactory, ServiceManagerFactory>(DILifeTime.Singleton);

            //Operative System
            uc.Register<IOperativeSystem, OperativeSystem>(DILifeTime.Singleton);

        }
    }
}
