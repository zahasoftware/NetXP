using NetXP.NetStandard.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NetXP.Netstandard.MVVM.XamarinForms
{
    public class ViewNavigator : IViewNavigator
    {
        private readonly INavigation navigation;

        public List<IView> Views => navigation.NavigationStack.Cast<IView>().ToList();

        public List<IView> Modals => navigation.ModalStack.Cast<IView>().ToList();

        public ViewNavigator(INavigation navigation)
        {
            this.navigation = navigation;
        }

        public async Task<IView> PopAsync(IView view)
        {
            var page = await navigation.PopAsync();
            return (IView)page;
        }

        public async Task PushAsync(IView view)
        {
            await navigation.PushAsync((Page)view);
        }
        public void RemovePage(IView view)
        {
            navigation.RemovePage((Page)view);
        }

        public async Task PushModal(IView view)
        {
            await navigation.PushModalAsync((Page)view);
        }

        public async Task<IView> PopModal(IView view)
        {
            return (IView)await navigation.PopModalAsync();
        }

    }
}
