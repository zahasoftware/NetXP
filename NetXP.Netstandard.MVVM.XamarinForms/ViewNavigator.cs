using NetXP.NetStandard.MVVM;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NetXP.Netstandard.MVVM.XamarinForms
{
    public class ViewNavigator : IViewNavigator
    {
        private readonly INavigation navigation;

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


    }
}
