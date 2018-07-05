using NetXP.NetStandard.Auditory;
using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Auditory.Implementations
{
    public static class AuditoryCompositionRoot
    {
        public static void RegisterAuditory(this IRegister uc)
        {
            //Auditory
            uc.Register<ILogger, Log4NetLogger>(DILifeTime.Singleton);
        }
    }
}
