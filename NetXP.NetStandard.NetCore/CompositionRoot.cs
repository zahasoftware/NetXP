using NetXP.NetStandard.Cryptography.Implementations;
using NetXP.NetStandard.Network;
using NetXP.NetStandard.Serialization.Implementations;
using NetXP.NetStandard.Auditory.Implementations;
using NetXP.NetStandard.Network.Services.Implementations;
using NetXP.NetStandard.DependencyInjection;
using Microsoft.Extensions.Options;
using NetXP.NetStandard.Network.SecureLittleProtocol.Implementation;

namespace NetXP.NetStandard.CompositionRoots
{
    public static class CompositionRoot
    {
        public static void RegisterAllNetXP(this IRegister cfg)
        {
            cfg.RegisterInstance(Options.Create(new PersistenPrivateKeyConfiguration()));
            cfg.RegisterInstance(Options.Create(new TCPOption()));

            cfg.RegisterNetXPStandard();

            cfg.RegisterSerialization();
            cfg.RegisterCryptography();
            cfg.RegisterAuditory();
            cfg.RegisterNetwork();
            cfg.RegisterNetworkServices();

            //Process
            cfg.Register<SystemInformation.ISystemInformation, SystemInformation.Implementations.SysInfo>();

        }
    }
}
