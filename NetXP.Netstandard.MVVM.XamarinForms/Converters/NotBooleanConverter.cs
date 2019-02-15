using System;
using Xamarin.Forms;

namespace NetXP.NetStandard.MVVM.XamarinForms.Converters
{
    public class NotBooleanConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool)) throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

    }
}
