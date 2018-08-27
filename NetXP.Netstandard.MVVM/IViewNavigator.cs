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
        void RemovePage(IView view);
        Task PushModal(IView view);
        Task<IView> PopModal(IView view);

        List<IView> Views { get; }
        List<IView> Modals { get; }
    }
}
