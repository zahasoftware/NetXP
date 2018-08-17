using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.MVVM;
using NetXP.NetStandard.MVVM.Implementations;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace NetXP.Netstandard.MVVM.XamarinForms
{
    public static class XamarinFormsMVVMCompositionRoot
    {
        public static void RegisterXamarinFormsMVVM(this IRegister r)
        {
            r.Register<IViewNavigator, ViewNavigator>();
            r.Register<ICommand, DelegateCommand>();
            r.Register<NetStandard.MVVM.IMessagingCenter, NetStandard.MVVM.XamarinForms.MessagingCenter>();
        }
    }
}
