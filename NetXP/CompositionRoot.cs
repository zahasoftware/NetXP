using System;
using NetXP.Serialization;
using NetXP.Cryptography;
using NetXP.DateAndTime;
using NetXP.DependencyInjection;
using NetXP.Compression.Implementations;
using Microsoft.Extensions.Options;
using NetXP.Reflection;
using NetXP.Reflection.Implementations;
using NetXP.DateAndTime.Implementation;
using NetXP.Compression;
using NetXP.Configuration;
using NetXP.Configuration.Implementations;
using Microsoft.Extensions.Configuration;
using System.IO;
using NetXP.Auditory;
using NetXP.Processes.Implementations;
using NetXP.SystemInformation;
using NetXP.SystemInformation.Implementations;
using NetXP.SystemManagers;
using NetXP.SystemManagers.Implementations;

namespace NetXP
{
    public static class CompositionRoot
    {
        public static void RegisterNetXP(this IRegister uc)
        {
            //cnf
            uc.Register<IConfigFactory, ConfigFactory>();

            //cmpr
            uc.Register<ICompression, DeflateCompress>(DILifeTime.Singleton);

            #region Processes

            //Process
            uc.Register<Processes.IIOTerminal, IOTerminal>();

            #endregion

            //Reflect
            uc.Register<IReflector, Reflector>(DILifeTime.Singleton);

            //System Information
            //ISysInfo need to be implemented in their os system.
            uc.Register<SystemInformation.IStorageInfo, SystemInformation.Implementations.SysInfo>();
            var customDateTime = new CustomDateTime(0);
            uc.RegisterInstance<ICustomDateTime>(customDateTime);
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
