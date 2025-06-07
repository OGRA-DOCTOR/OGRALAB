using System.Collections.Generic;
using System.Threading.Tasks;
using OGRALAB.Models;
using OGRALAB.Enums;
namespace OGRALAB.Services
{
    /// <summary>
    /// واجهة خدمة إدارة المستخدمين
    /// تتضمن جميع العمليات المتعلقة بإدارة حسابات المستخدمين والصلاحيات
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>
        /// جلب جميع المستخدمين في النظام
        /// </summary>
        /// <returns>قائمة بجميع المستخدمين</returns>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>
        /// جلب مستخدم بواسطة المعرف الفريد
        /// </summary>
        /// <param name="id">معرف المستخدم</param>
        /// <returns>المستخدم أو null إذا لم يوجد</returns>
        Task<User?> GetUserByIdAsync(int id);

        /// <summary>
        /// جلب مستخدم بواسطة اسم المستخدم
        /// </summary>
        /// <param name="username">اسم المستخدم</param>
        /// <returns>المستخدم أو null إذا لم يوجد</returns>
        Task<User?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// إضافة مستخدم جديد إلى النظام
        /// </summary>
        /// <param name="username">اسم المستخدم</param>
        /// <param name="password">كلمة المرور</param>
        /// <param name="role">نوع الصلاحيات</param>
        /// <param name="fullName">الاسم الكامل (اختياري)</param>
        /// <param name="email">البريد الإلكتروني (اختياري)</param>
        /// <returns>true إذا تمت الإضافة بنجاح</returns>
        Task<bool> AddUserAsync(string username, string password, UserRole role, string? fullName = null, string? email = null);

        /// <summary>
        /// تحديث بيانات مستخدم موجود
        /// </summary>
        /// <param name="id">معرف المستخدم</param>
        /// <param name="username">اسم المستخدم الجديد</param>
        /// <param name="role">نوع الصلاحيات الجديد</param>
        /// <param name="fullName">الاسم الكامل الجديد (اختياري)</param>
        /// <param name="email">البريد الإلكتروني الجديد (اختياري)</param>
        /// <returns>true إذا تم التحديث بنجاح</returns>
        Task<bool> UpdateUserAsync(int id, string username, UserRole role, string? fullName = null, string? email = null);

        /// <summary>
        /// تغيير كلمة مرور مستخدم
        /// </summary>
        /// <param name="id">معرف المستخدم</param>
        /// <param name="newPassword">كلمة المرور الجديدة</param>
        /// <returns>true إذا تم التغيير بنجاح</returns>
        Task<bool> ChangePasswordAsync(int id, string newPassword);

        /// <summary>
        /// حذف مستخدم من النظام
        /// ملاحظة: لا يمكن حذف المسؤول الوحيد أو المستخدم المسجل دخوله حالياً
        /// </summary>
        /// <param name="id">معرف المستخدم</param>
        /// <returns>true إذا تم الحذف بنجاح</returns>
        Task<bool> DeleteUserAsync(int id);

        /// <summary>
        /// تفعيل أو إلغاء تفعيل مستخدم
        /// </summary>
        /// <param name="id">معرف المستخدم</param>
        /// <returns>true إذا تم تغيير الحالة بنجاح</returns>
        Task<bool> ToggleUserActiveStatusAsync(int id);

        /// <summary>
        /// فحص ما إذا كان اسم المستخدم متاحاً أم مستخدم
        /// </summary>
        /// <param name="username">اسم المستخدم المراد فحصه</param>
        /// <param name="excludeUserId">معرف المستخدم المراد استثناؤه من الفحص (للتحديث)</param>
        /// <returns>true إذا كان اسم المستخدم متاحاً</returns>
        Task<bool> IsUsernameAvailableAsync(string username, int? excludeUserId = null);

        /// <summary>
        /// فحص صحة كلمة المرور حسب المعايير المطلوبة
        /// </summary>
        /// <param name="password">كلمة المرور</param>
        /// <returns>true إذا كانت كلمة المرور صحيحة</returns>
        bool ValidatePassword(string password);

        /// <summary>
        /// تشفير كلمة المرور باستخدام BCrypt
        /// </summary>
        /// <param name="password">كلمة المرور الأصلية</param>
        /// <returns>كلمة المرور المشفرة</returns>
        string HashPassword(string password);

        /// <summary>
        /// التحقق من كلمة المرور مقابل النسخة المشفرة
        /// </summary>
        /// <param name="password">كلمة المرور الأصلية</param>
        /// <param name="hashedPassword">كلمة المرور المشفرة</param>
        /// <returns>true إذا كانت كلمة المرور صحيحة</returns>
        bool VerifyPassword(string password, string hashedPassword);

        /// <summary>
        /// الحصول على عدد المسؤولين النشطين في النظام
        /// </summary>
        /// <returns>عدد المسؤولين النشطين</returns>
        Task<int> GetActiveAdminCountAsync();

        /// <summary>
        /// فحص ما إذا كان يمكن حذف المستخدم
        /// (لا يمكن حذف المسؤول الوحيد أو المستخدم الحالي)
        /// </summary>
        /// <param name="userId">معرف المستخدم</param>
        /// <param name="currentUserId">معرف المستخدم الحالي</param>
        /// <returns>true إذا كان يمكن حذف المستخدم</returns>
        Task<bool> CanDeleteUserAsync(int userId, int? currentUserId = null);

        /// <summary>
        /// فحص ما إذا كان يمكن إلغاء تفعيل المستخدم
        /// (لا يمكن إلغاء تفعيل المسؤول الوحيد)
        /// </summary>
        /// <param name="userId">معرف المستخدم</param>
        /// <returns>true إذا كان يمكن إلغاء تفعيل المستخدم</returns>
        Task<bool> CanDeactivateUserAsync(int userId);
    }
}