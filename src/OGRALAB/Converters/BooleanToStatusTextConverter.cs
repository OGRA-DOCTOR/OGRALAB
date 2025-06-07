using System;
using System.Globalization;
using System.Windows.Data;
namespace OGRALAB.Converters
{
    public class BooleanToStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasResult)
            {
                return hasResult ? "✅ مكتمل" : "⏳ معلق";
            }
            return "غير محدد";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}