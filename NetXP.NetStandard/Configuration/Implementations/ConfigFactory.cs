using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Configuration.Implementations
{
    public class ConfigFactory : IConfigFactory
    {
        private readonly IContainer uc;

        public ConfigFactory(IContainer uc)
        {
            this.uc = uc;
        }

        public IConfig Resolve(ConfigType configType = ConfigType.Basic)
        {
            if (configType != ConfigType.Basic)
            {
                return this.uc.Resolve<IConfig>(configType.ToString());
            }
            else
            {
                return this.uc.Resolve<IConfig>();
            }
        }
    }
}
