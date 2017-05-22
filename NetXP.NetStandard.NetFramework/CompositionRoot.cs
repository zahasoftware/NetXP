using NetXP.NetStandard.Cryptography.Implementations;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.NetFramework.Cryptography;
using NetXP.NetStandard.NetFramework.Cryptography.Implementations;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Network.TCP.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.NetFramework
{
    public static class CompositionRoot
    {
        public static void Init(IRegister cfg)
        {
            NetXP.NetStandard.CompositionRoot.Init(cfg);

            //Cryptography
            cfg.Register<NetStandard.Cryptography.ISymetricCrypt, Symetric>();
            cfg.Register<NetStandard.Cryptography.IAsymetricCrypt, AsymetricCryptWithOpenSSL>();

            //Mail
            cfg.Register<NetStandard.Network.Email.IMailSender, Network.Email.MailSender>();

        }
    }
}
