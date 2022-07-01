using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.DependencyInjection
{
    public interface IContainerFactory
    {
        IContainer Create();
    }
}
