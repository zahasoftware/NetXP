using NetXP.NetStandard.MVVM;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.MVVM.XamarinForms
{
    public class MessagingCenter : NetXP.NetStandard.MVVM.IMessagingCenter
    {
        public void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
        {
            Xamarin.Forms.MessagingCenter.Send<TSender, TArgs>(sender, message, args);
        }

        public void Send<TSender>(TSender sender, string message) where TSender : class
        {
            Xamarin.Forms.MessagingCenter.Send<TSender>(sender, message);
        }

        public void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = null) where TSender : class
        {
            Xamarin.Forms.MessagingCenter.Subscribe<TSender, TArgs>(this, message, callback, source);
        }

        public void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = null) where TSender : class
        {
            Xamarin.Forms.MessagingCenter.Subscribe<TSender>(this, message, callback, source);
        }

        public void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
        {
            Xamarin.Forms.MessagingCenter.Unsubscribe<TSender, TArgs>(this, message);
        }

        public void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
        {
            Xamarin.Forms.MessagingCenter.Unsubscribe<TSender>(this, message);
        }
    }
}
