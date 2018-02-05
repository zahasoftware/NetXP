using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.NetCore
{
    public static class CompositionRoot
    {
        public static void AddNetXPNetCoreRegisters(this IRegister cfg, IContainer container, string appSettingFile = null)
        {
            NetXP.NetStandard.CompositionRoot.RegisterNetXPStandard(cfg, container, appSettingFile);

            //Cryptography
            cfg.Register<NetStandard.Cryptography.ISymetricCrypt, Cryptography.Implementations.SymetricAes>();
            cfg.Register<NetStandard.Cryptography.IAsymetricCrypt, Cryptography.Implementations.AsymetricCryptWithMSRSA>();

            //Process
            cfg.Register<NetStandard.SystemInformation.ISystemInformation, NetStandard.SystemInformation.Implementations.SysInfo>();

        }
    }
}
