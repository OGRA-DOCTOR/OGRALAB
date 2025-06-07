using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
namespace OGRALAB.Converters
{
    public class BooleanToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasResult)
            {
                return hasResult ?
                    new SolidColorBrush(Color.FromRgb(56, 161, 105)) : // أخضر للمكتمل
                    new SolidColorBrush(Color.FromRgb(237, 137, 54));   // برتقالي للمعلق
            }
            return new SolidColorBrush(Colors.Gray);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}