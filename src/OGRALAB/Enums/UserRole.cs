using System.ComponentModel.DataAnnotations;
namespace OGRALAB.Enums
{
    /// <summary>
    /// أنواع صلاحيات المستخدمين في النظام
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// مستخدم عادي - صلاحيات محدودة
        /// يمكنه: إدخال المرضى، إدخال النتائج، عرض التقارير، إدارة التحاليل
        /// لا يمكنه: إدارة المستخدمين
        /// </summary>
        [Display(Name = "مستخدم عادي")]
        User = 0,

        /// <summary>
        /// مسؤول - صلاحيات كاملة
        /// يمكنه: جميع صلاحيات المستخدم العادي + إدارة المستخدمين
        /// </summary>
        [Display(Name = "مسؤول")]
        Admin = 1
    }
}