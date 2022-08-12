using NetXP.Cryptography.Implementations;
using NetXP.Network;
using NetXP.Serialization.Implementations;
using NetXP.Auditory.Implementations;
using NetXP.Network.Services.Implementations;
using NetXP.DependencyInjection;
using Microsoft.Extensions.Options;
using NetXP.Network.SecureLittleProtocol.Implementation;

namespace NetXP.CompositionRoots
{
    public static class CompositionRoot
    {
        public static void RegisterAllNetXP(this IRegister cfg)
        {
            cfg.RegisterInstance(Options.Create(new PersistenPrivateKeyConfiguration()));
            cfg.RegisterInstance(Options.Create(new TCPOption()));

            cfg.RegisterNetXP();

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
