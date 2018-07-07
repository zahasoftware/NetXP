using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.MVVM;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.Netstandard.MVVM.XamarinForms
{
    public static class XamarinFormsMVVMCompositionRoot
    {
        public static void RegisterXamarinFormsMVVM(this IRegister r)
        {
            r.Register<IViewNavigator, ViewNavigator>();
        }
    }
}
