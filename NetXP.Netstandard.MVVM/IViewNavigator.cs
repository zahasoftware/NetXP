using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.MVVM
{
    public interface IViewNavigator
    {
        Task PushAsync(IView view);
        Task<IView> PopAsync(IView view);
    }
}
