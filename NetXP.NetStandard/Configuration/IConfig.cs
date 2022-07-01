using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Configuration
{
    ///Class replace with: http://developer.telerik.com/featured/new-configuration-model-asp-net-core/
    ///and more: https://www.danylkoweb.com/Blog/no-configurationmanager-in-aspnet-core-GC
    public interface IConfig
    {
        string Get(string key);
        void Set(string key, string newValue);
    }
}
