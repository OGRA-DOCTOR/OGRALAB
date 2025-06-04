using System;
using System.ComponentModel.DataAnnotations;

namespace OGRALAB.Models
{
    /// <summary>
    /// نموذج طلب الفحص المخبري
    /// </summary>
    public class TestRequest
    {
        /// <summary>
        /// المعرف الفريد لطلب الفحص
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// معرف المريض
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// المريض المرتبط
        /// </summary>
        public virtual Patient Patient { get; set; } = null!;

        /// <summary>
        /// معرف الفحص
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// الفحص المرتبط
        /// </summary>
        public virtual Test Test { get; set; } = null!;

        /// <summary>
        /// السعر المدفوع للفحص
        /// </summary>
        public decimal PaidPrice { get; set; }

        /// <summary>
        /// حالة الفحص (مطلوب، جاري التنفيذ، مكتمل، ملغي)
        /// </summary>
        [StringLength(50)]
        public string Status { get; set; } = "Requested";

        /// <summary>
        /// تاريخ الطلب
        /// </summary>
        public DateTime RequestedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// تاريخ أخذ العينة
        /// </summary>
        public DateTime? SampleCollectedDate { get; set; }

        /// <summary>
        /// تاريخ إكمال الفحص
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// ملاحظات
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// هل تم الانتهاء من الفحص
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// هل تمت مراجعة النتيجة
        /// </summary>
        public bool IsReviewed { get; set; }

        /// <summary>
        /// هل تم طباعة النتيجة
        /// </summary>
        public bool IsPrinted { get; set; }

        /// <summary>
        /// هل تم تصدير النتيجة
        /// </summary>
        public bool IsExported { get; set; }

        /// <summary>
        /// نتيجة الفحص المرتبطة
        /// </summary>
        public virtual TestResult? TestResult { get; set; }
    }
}
