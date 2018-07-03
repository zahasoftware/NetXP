using NetXP.NetStandard.Auditory;
using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Auditory.Implementations
{
    class AuditoryCompositionRoot
    {
        public static void Init(IRegister uc)
        {
            //Auditory
            uc.Register<ILogger, Log4NetLogger>(DILifeTime.Singleton);
        }
    }
}
