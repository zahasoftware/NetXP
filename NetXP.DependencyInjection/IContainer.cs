using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.DependencyInjection
{
    public interface IContainer : IResolver, IDisposable
    {
        string Name { get; set; }
        IConfiguration Configuration { get; }
    }
}
