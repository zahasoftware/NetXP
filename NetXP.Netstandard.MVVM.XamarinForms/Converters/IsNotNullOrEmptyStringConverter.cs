using System;
using System.Globalization;
using Xamarin.Forms;

namespace NetXP.NetStandard.MVVM.XamarinForms.Converters
{
    public class IsNotNullOrEmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
