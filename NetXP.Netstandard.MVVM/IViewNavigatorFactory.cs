using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.MVVM
{
    public interface IViewNavigatorFactory
    {
        IViewNavigator Resolve(string name = null);
    }
}
