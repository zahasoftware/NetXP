using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.MVVM
{
    public interface IXViewFactory
    {
        IView Resolve(string view);
    }
}
