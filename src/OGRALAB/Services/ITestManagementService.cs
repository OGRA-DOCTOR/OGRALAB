using System.Collections.Generic;
using System.Threading.Tasks;
using OGRALAB.Models;
using OGRALAB.Enums;
namespace OGRALAB.Services
{
    /// <summary>
    /// واجهة خدمة إدارة أنواع التحاليل والمعدلات الطبيعية المرنة
    /// تتضمن جميع العمليات المتعلقة بإدارة التحاليل ومعدلاتها الطبيعية
    /// </summary>
    public interface ITestManagementService
    {
        #region إدارة التحاليل الأساسية
        /// <summary>
        /// جلب جميع التحاليل النشطة في النظام
        /// </summary>
        /// <returns>قائمة بجميع التحاليل النشطة</returns>
        Task<List<Test>> GetAllTestsAsync();

        /// <summary>
        /// جلب تحليل بواسطة المعرف الفريد
        /// </summary>
        /// <param name="id">معرف التحليل</param>
        /// <returns>التحليل أو null إذا لم يوجد</returns>
        Task<Test?> GetTestByIdAsync(int id);

        /// <summary>
        /// إضافة تحليل جديد إلى النظام
        /// </summary>
        /// <param name="name">اسم التحليل</param>
        /// <param name="unit">وحدة القياس (اختياري)</param>
        /// <param name="price">سعر التحليل</param>
        /// <param name="description">وصف التحليل (اختياري)</param>
        /// <param name="category">فئة التحليل (اختياري)</param>
        /// <returns>true إذا تمت الإضافة بنجاح</returns>
        Task<bool> AddTestAsync(string name, string? unit, decimal price, string? description = null, string? category = null);

        /// <summary>
        /// تحديث بيانات تحليل موجود
        /// </summary>
        /// <param name="id">معرف التحليل</param>
        /// <param name="name">اسم التحليل الجديد</param>
        /// <param name="unit">وحدة القياس الجديدة (اختياري)</param>
        /// <param name="price">السعر الجديد</param>
        /// <param name="description">الوصف الجديد (اختياري)</param>
        /// <param name="category">الفئة الجديدة (اختياري)</param>
        /// <returns>true إذا تم التحديث بنجاح</returns>
        Task<bool> UpdateTestAsync(int id, string name, string? unit, decimal price, string? description = null, string? category = null);

        /// <summary>
        /// حذف تحليل من النظام
        /// ملاحظة: لا يمكن حذف التحليل إذا كان له نتائج مرتبطة
        /// </summary>
        /// <param name="id">معرف التحليل</param>
        /// <returns>true إذا تم الحذف بنجاح</returns>
        Task<bool> DeleteTestAsync(int id);

        /// <summary>
        /// فحص ما إذا كان اسم التحليل متاحاً أم مستخدم
        /// </summary>
        /// <param name="name">اسم التحليل المراد فحصه</param>
        /// <param name="excludeTestId">معرف التحليل المراد استثناؤه من الفحص (للتحديث)</param>
        /// <returns>true إذا كان اسم التحليل متاحاً</returns>
        Task<bool> IsTestNameAvailableAsync(string name, int? excludeTestId = null);

        /// <summary>
        /// فحص ما إذا كان يمكن حذف التحليل (ليس له نتائج مرتبطة)
        /// </summary>
        /// <param name="testId">معرف التحليل</param>
        /// <returns>true إذا كان يمكن حذف التحليل</returns>
        Task<bool> CanDeleteTestAsync(int testId);
        #endregion
        #region إدارة المعدلات الطبيعية المرنة
        /// <summary>
        /// جلب جميع المعدلات الطبيعية لتحليل معين
        /// </summary>
        /// <param name="testId">معرف التحليل</param>
        /// <returns>قائمة بالمعدلات الطبيعية مرتبة</returns>
        Task<List<TestReferenceRange>> GetTestReferenceRangesAsync(int testId);

        /// <summary>
        /// إضافة معدل طبيعي جديد لتحليل
        /// </summary>
        /// <param name="testId">معرف التحليل</param>
        /// <param name="gender">الجنس المطبق عليه المعدل</param>
        /// <param name="ageOperator">نوع مقارنة العمر</param>
        /// <param name="ageValue1">القيمة الأولى للعمر</param>
        /// <param name="ageValue2">القيمة الثانية للعمر (للنطاق فقط)</param>
        /// <param name="referenceValue">القيمة المرجعية</param>
        /// <param name="notes">ملاحظات إضافية (اختياري)</param>
        /// <returns>true إذا تمت الإضافة بنجاح</returns>
        Task<bool> AddReferenceRangeAsync(int testId, Gender gender, AgeOperator ageOperator,
            int ageValue1, int? ageValue2, string referenceValue, string? notes = null);

        /// <summary>
        /// تحديث معدل طبيعي موجود
        /// </summary>
        /// <param name="id">معرف المعدل الطبيعي</param>
        /// <param name="gender">الجنس الجديد</param>
        /// <param name="ageOperator">نوع مقارنة العمر الجديد</param>
        /// <param name="ageValue1">القيمة الأولى للعمر الجديدة</param>
        /// <param name="ageValue2">القيمة الثانية للعمر الجديدة (للنطاق فقط)</param>
        /// <param name="referenceValue">القيمة المرجعية الجديدة</param>
        /// <param name="notes">الملاحظات الجديدة (اختياري)</param>
        /// <returns>true إذا تم التحديث بنجاح</returns>
        Task<bool> UpdateReferenceRangeAsync(int id, Gender gender, AgeOperator ageOperator,
            int ageValue1, int? ageValue2, string referenceValue, string? notes = null);

        /// <summary>
        /// حذف معدل طبيعي من النظام
        /// </summary>
        /// <param name="id">معرف المعدل الطبيعي</param>
        /// <returns>true إذا تم الحذف بنجاح</returns>
        Task<bool> DeleteReferenceRangeAsync(int id);

        /// <summary>
        /// الحصول على المعدل الطبيعي المناسب لمريض معين
        /// </summary>
        /// <param name="testId">معرف التحليل</param>
        /// <param name="age">عمر المريض</param>
        /// <param name="gender">جنس المريض</param>
        /// <returns>المعدل الطبيعي المناسب أو المعدل العام</returns>
        Task<string> GetAppropriateReferenceRangeAsync(int testId, int age, Gender gender);

        /// <summary>
        /// فحص صحة بيانات المعدل الطبيعي قبل الحفظ
        /// </summary>
        /// <param name="ageOperator">نوع مقارنة العمر</param>
        /// <param name="ageValue1">القيمة الأولى للعمر</param>
        /// <param name="ageValue2">القيمة الثانية للعمر</param>
        /// <returns>true إذا كانت البيانات صحيحة</returns>
        bool ValidateReferenceRangeData(AgeOperator ageOperator, int ageValue1, int? ageValue2);
        #endregion
        #region وظائف مساعدة
        /// <summary>
        /// البحث في التحاليل بواسطة النص
        /// </summary>
        /// <param name="searchText">النص المراد البحث عنه</param>
        /// <returns>قائمة بالتحاليل المطابقة</returns>
        Task<List<Test>> SearchTestsAsync(string searchText);

        /// <summary>
        /// جلب التحاليل بواسطة الفئة
        /// </summary>
        /// <param name="category">فئة التحليل</param>
        /// <returns>قائمة بالتحاليل في الفئة المحددة</returns>
        Task<List<Test>> GetTestsByCategoryAsync(string category);

        /// <summary>
        /// جلب جميع فئات التحاليل الموجودة
        /// </summary>
        /// <returns>قائمة بفئات التحاليل</returns>
        Task<List<string>> GetTestCategoriesAsync();

        /// <summary>
        /// جلب إحصائيات عامة عن التحاليل
        /// </summary>
        /// <returns>معلومات إحصائية</returns>
        Task<(int TotalTests, int ActiveTests, int TestsWithReferenceRanges)> GetTestStatisticsAsync();
        #endregion
    }
}