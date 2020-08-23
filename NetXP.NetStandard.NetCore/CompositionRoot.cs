using NetXP.NetStandard.Cryptography.Implementations;
using NetXP.NetStandard.Network;
using NetXP.NetStandard.Serialization.Implementations;
using NetXP.NetStandard.Auditory.Implementations;
using System;
using System.Collections.Generic;
using System.Text;
using NetXP.NetStandard.Network.Services.Implementations;

namespace NetXP.NetStandard.DependencyInjection
{
    public static class CompositionRoot
    {
        public static void RegisterAllNetXP(this IRegister cfg, string appSettingFile = null)
        {
            cfg.RegisterNetXPStandard(appSettingFile);

            cfg.RegisterSerialization();
            cfg.RegisterCryptography();
            cfg.RegisterNetwork(appSettingFile);
            cfg.RegisterAuditory();
            cfg.RegisterNetworkServices();

            //Process
            cfg.Register<SystemInformation.ISystemInformation, SystemInformation.Implementations.SysInfo>();

        }
    }
}
