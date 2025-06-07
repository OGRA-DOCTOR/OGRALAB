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
            var enumValue = value.ToString();
            try
            {
                var fieldInfo = enumType.GetField(enumValue!);
                if (fieldInfo == null)
                    return enumValue!;
                var displayAttribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();

                return displayAttribute?.Name ?? enumValue!;
            }
            catch
            {
                return enumValue!;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // هذا المحول للعرض فقط - لا يدعم التحويل العكسي
            throw new NotImplementedException("EnumToDisplayNameConverter is for display purposes only.");
        }
    }
    /// <summary>
    /// محول مُبسط لبعض التعدادات الشائعة في النظام
    /// يوفر أسماء عرض مباشرة بدون الحاجة لـ Display attributes
    /// </summary>
    public class SimpleEnumToArabicConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            // التعامل مع Gender enum
            if (value is OGRALAB.Enums.Gender gender)
            {
                return gender switch
                {
                    OGRALAB.Enums.Gender.Male => "ذكر",
                    OGRALAB.Enums.Gender.Female => "أنثى",
                    _ => "غير محدد"
                };
            }
            // التعامل مع UserRole enum
            if (value is OGRALAB.Enums.UserRole role)
            {
                return role switch
                {
                    OGRALAB.Enums.UserRole.Admin => "مسؤول",
                    OGRALAB.Enums.UserRole.User => "مستخدم عادي",
                    _ => "غير محدد"
                };
            }
            // التعامل مع AgeOperator enum
            if (value is OGRALAB.Enums.AgeOperator ageOperator)
            {
                return ageOperator switch
                {
                    OGRALAB.Enums.AgeOperator.Range => "نطاق (من - إلى)",
                    OGRALAB.Enums.AgeOperator.GreaterThan => "أكبر من",
                    OGRALAB.Enums.AgeOperator.LessThan => "أقل من",
                    OGRALAB.Enums.AgeOperator.Equals => "يساوي",
                    OGRALAB.Enums.AgeOperator.GreaterThanOrEqual => "أكبر من أو يساوي",
                    OGRALAB.Enums.AgeOperator.LessThanOrEqual => "أقل من أو يساوي",
                    _ => "غير محدد"
                };
            }
            // إذا لم يكن من الأنواع المدعومة، استخدم اسم التعداد مباشرة
            return value.ToString() ?? string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("SimpleEnumToArabicConverter is for display purposes only.");
        }
    }
    /// <summary>
    /// محول خاص للبيانات المنطقية إلى نصوص عربية
    /// مفيد لعرض حالات التفعيل والحالات المنطقية الأخرى
    /// </summary>
    public class BooleanToArabicTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // يمكن تخصيص النص بناءً على parameter
                var customTexts = parameter?.ToString()?.Split('|');

                if (customTexts?.Length == 2)
                {
                    return boolValue ? customTexts[0] : customTexts[1];
                }
                // النصوص الافتراضية
                return boolValue ? "نشط" : "غير نشط";
            }
            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("BooleanToArabicTextConverter is for display purposes only.");
        }
    }
}