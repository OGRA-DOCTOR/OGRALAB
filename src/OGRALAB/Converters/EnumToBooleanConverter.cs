using System;
using System.Globalization;
using System.Windows.Data;

namespace OGRALAB.Converters
{
    /// <summary>
    /// محول محسن لتحويل قيمة Enum إلى Boolean للاستخدام مع RadioButton
    /// يدعم String parameters وEnum values
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // إذا كان parameter عبارة عن string، حاول تحويله إلى نفس نوع value
            if (parameter is string stringParameter && value is Enum enumValue)
            {
                try
                {
                    var enumType = enumValue.GetType();
                    var parsedParameter = Enum.Parse(enumType, stringParameter, true);
                    return enumValue.Equals(parsedParameter);
                }
                catch
                {
                    return false;
                }
            }

            // المقارنة المباشرة
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
            {
                // إذا كان parameter عبارة عن string، حاول تحويله إلى targetType
                if (parameter is string stringParameter && targetType.IsEnum)
                {
                    try
                    {
                        return Enum.Parse(targetType, stringParameter, true);
                    }
                    catch
                    {
                        return Binding.DoNothing;
                    }
                }

                // إذا كان parameter من نفس نوع targetType
                if (targetType.IsAssignableFrom(parameter.GetType()))
                {
                    return parameter;
                }
            }

            return Binding.DoNothing;
        }
    }
}