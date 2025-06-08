using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace OGRALAB.Converters
{
    /// <summary>
    /// محول لتحويل قيم Enum إلى أسماء العرض الخاصة بها من Display attribute
    /// يستخدم للعرض في ComboBox وListBox وغيرها
    /// </summary>
    public class EnumToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var enumType = value.GetType();
            var enumValueName = value.ToString();

            if (enumValueName == null)
                return string.Empty;

            try
            {
                var fieldInfo = enumType.GetField(enumValueName);
                if (fieldInfo == null)
                    return enumValueName;

                var displayAttribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();
                return displayAttribute?.Name ?? enumValueName;
            }
            catch
            {
                return enumValueName;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // هذا المحول للعرض فقط - لا يدعم التحويل العكسي
            throw new NotImplementedException("EnumToDisplayNameConverter is for display purposes only.");
        }
    }
}