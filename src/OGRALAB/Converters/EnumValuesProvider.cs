using System;
using System.Linq;
using System.Windows.Markup;

namespace OGRALAB.Converters
{
    public class EnumValuesProvider : MarkupExtension
    {
        private readonly Type _enumType;

        public EnumValuesProvider(Type enumType)
        {
            if (enumType == null || !enumType.IsEnum)
                throw new ArgumentException("Type must be an Enum type.");

            _enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // يُرجع قيم الـ enum مباشرة
            return Enum.GetValues(_enumType);
        }
    }
}