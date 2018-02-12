using NetXP.NetStandard.Cryptography.Implementations;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.NetFramework.Cryptography;
using NetXP.NetStandard.NetFramework.Cryptography.Implementations;
using NetXP.NetStandard.Network.TCP;
using NetXP.NetStandard.Network.TCP.Implementations;
using SmartSecurity.NetStandard.NetFramework.SystemInformation.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.NetFramework
{
    public static class CompositionRoot
    {
        public static void AddNetXPNetFrameworkRegisters(this IRegister cfg, IContainer container, string appSettingFile = null)
        {
            cfg.RegisterNetXPStandard(container, appSettingFile);



            //Cryptography
            cfg.Register<NetStandard.Cryptography.ISymetricCrypt, Symetric>();
            cfg.Register<NetStandard.Cryptography.IAsymetricCrypt, AsymetricCryptWithOpenSSL>();

            //Mail
            cfg.Register<NetStandard.Network.Email.IMailSender, Network.Email.Implementations.MailSender>();

            //SystemInfo
            cfg.Register<NetStandard.SystemInformation.ISystemInformation, SystemInformation.Implementations.SysInfo>();
            cfg.Register<NetStandard.SystemInformation.IServiceInformer, ServiceInformerWindowsOnly>();


        }
    }
}
