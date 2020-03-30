using NetXP.NetStandard.Cryptography.Implementations;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Network;
using NetXP.NetStandard.Serialization.Implementations;
using NetXP.NetStandard.Auditory.Implementations;
using System;
using System.Collections.Generic;
using System.Text;
using NetXP.NetStandard.Network.Services.Implementations;

namespace NetXP.NetStandard.NetCore
{
    public static class CompositionRoot
    {
        public static void AddNetXPNetCoreRegisters(this IRegister cfg, IContainer container, string appSettingFile = null)
        {
            cfg.RegisterNetXPStandard(container, appSettingFile);

            cfg.RegisterSerialization();
            cfg.RegisterCryptography();
            cfg.RegisterNetwork(container, appSettingFile);
            cfg.RegisterAuditory();
            cfg.RegisterNetworkServices();

            //Cryptography
            cfg.Register<NetStandard.Cryptography.ISymetricCrypt, NetStandard.Cryptography.Implementations.SymetricAes>();
            cfg.Register<NetStandard.Cryptography.IAsymetricCrypt, Cryptography.Implementations.AsymetricCryptWithMSRSA>();

            //Process
            cfg.Register<NetStandard.SystemInformation.ISystemInformation, NetStandard.SystemInformation.Implementations.SysInfo>();

        }
    }
}
