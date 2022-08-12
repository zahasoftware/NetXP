﻿using NetXP.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Configuration.Implementations
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
            if (configType == ConfigType.None)
            {
                return this.uc.Resolve<IConfig>();
            }
            else
            {
                return this.uc.Resolve<IConfig>(configType.ToString());
            }
        }
    }
}
