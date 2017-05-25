using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.NetCore
{
    public static class CompositionRoot
    {
        public static void RegisterNetXPCore(this IRegister cfg)
        {
            NetXP.NetStandard.CompositionRoot.RegisterNetXPStandard(cfg);

            //Cryptography
            cfg.Register<NetStandard.Cryptography.ISymetricCrypt, Cryptography.Implementations.Symetric>();
            cfg.Register<NetStandard.Cryptography.IAsymetricCrypt, Cryptography.Implementations.AsymetricCryptWithMSRSA>();

            //Mail
            cfg.Register<NetStandard.Network.Email.IMailSender, Network.Email.Implementations.MailKitWrapper>();

        }
    }
}
