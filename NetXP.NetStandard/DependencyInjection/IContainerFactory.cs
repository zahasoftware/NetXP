using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.DependencyInjection
{
    public interface IContainerFactory
    {
        IContainer Create();
    }
}
