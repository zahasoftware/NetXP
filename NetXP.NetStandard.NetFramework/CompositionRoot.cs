using NetXP.NetStandard.Auditory.Implementations;
using NetXP.NetStandard.Cryptography.Implementations;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.NetFramework.Cryptography.Implementations;
using NetXP.NetStandard.Serialization.Implementations;
using SmartSecurity.NetStandard.NetFramework.SystemInformation.Implementations;
using NetXP.NetStandard.Network;

namespace NetXP.NetStandard.NetFramework
{
    public static class CompositionRoot
    {
        public static void AddNetXPNetFrameworkRegisters(this IRegister cfg, IContainer container, string appSettingFile = null)
        {
            cfg.RegisterNetXPStandard(container, appSettingFile);
            cfg.RegisterSerialization();
            cfg.RegisterCryptography();
            cfg.RegisterNetwork(container, appSettingFile);
            cfg.RegisterAuditory();

            //Cryptography
            cfg.Register<NetStandard.Cryptography.ISymetricCrypt, Symetric>();
            cfg.Register<NetStandard.Cryptography.IAsymetricCrypt, AsymetricCryptWithOpenSSL>();

            //SystemInfo
            cfg.Register<NetStandard.SystemInformation.ISystemInformation, SystemInformation.Implementations.SysInfo>();
            cfg.Register<NetStandard.SystemInformation.IServiceInformer, ServiceInformerWindowsOnly>();


        }
    }
}
