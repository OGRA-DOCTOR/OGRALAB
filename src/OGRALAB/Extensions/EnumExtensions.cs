using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using OGRALAB.Enums;

namespace OGRALAB.Extensions
{
    // --- الكلاس الأول: للتعدادات بشكل عام ---
    public static class EnumGeneralExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            if (fieldInfo == null) return enumValue.ToString();
            var displayAttribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }

    // --- الكلاس الثاني: خاص بـ UserRole ---
    public static class UserRoleExtensions
    {
        public static bool CanManageUsers(this UserRole role)
        {
            return role == UserRole.Admin;
        }
    }

    // --- الكلاس الثالث: خاص بـ AgeOperator ---
    public static class AgeOperatorExtensions
    {
        public static bool RequiresSecondValue(this AgeOperator ageOperator)
        {
            return ageOperator == AgeOperator.Range;
        }
    }
}