using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
namespace OGRALAB.Converters
{
    public class AbnormalResultColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAbnormal)
            {
                return isAbnormal ?
                    new SolidColorBrush(Color.FromRgb(229, 62, 62)) :  // أحمر للنتائج غير الطبيعية
                    new SolidColorBrush(Color.FromRgb(45, 55, 72));    // رمادي داكن للنتائج الطبيعية
            }
            return new SolidColorBrush(Colors.Black);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}