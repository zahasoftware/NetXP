using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.DependencyInjection
{
    public interface IContainer : IResolver, IDisposable
    {
        string Name { get; set; }
        IConfiguration Configuration { get; }
    }
}
