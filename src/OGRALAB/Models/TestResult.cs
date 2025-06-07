using System;
using System.ComponentModel.DataAnnotations;
using OGRALAB.Enums;

namespace OGRALAB.Models
{
    /// <summary>
    /// نموذج نتيجة الفحص المخبري
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// المعرف الفريد لنتيجة الفحص
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// معرف طلب الفحص
        /// </summary>
        public int TestRequestId { get; set; }

        /// <summary>
        /// طلب الفحص المرتبط
        /// </summary>
        public virtual TestRequest TestRequest { get; set; } = null!;

        /// <summary>
        /// معرف الفحص
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// الفحص المرتبط
        /// </summary>
        public virtual Test Test { get; set; } = null!;

        /// <summary>
        /// النتيجة النصية
        /// </summary>
        [StringLength(500)]
        public string TextResult { get; set; } = string.Empty;

        /// <summary>
        /// النتيجة الرقمية
        /// </summary>
        public decimal? NumericResult { get; set; }

        /// <summary>
        /// علامة الفحص (طبيعي، مرتفع، منخفض، حرج)
        /// </summary>
        public TestFlag Flag { get; set; } = TestFlag.Normal;

        /// <summary>
        /// الوحدة المستخدمة في النتيجة
        /// </summary>
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// المعدل الطبيعي المطبق
        /// </summary>
        [StringLength(100)]
        public string AppliedNormalRange { get; set; } = string.Empty;

        /// <summary>
        /// تعليقات خاصة بهذا الفحص
        /// </summary>
        [StringLength(1000)]
        public string Comments { get; set; } = string.Empty;

        /// <summary>
        /// تاريخ إدخال النتيجة
        /// </summary>
        public DateTime ResultDate { get; set; } = DateTime.Now;

        /// <summary>
        /// اسم المستخدم الذي أدخل النتيجة
        /// </summary>
        [StringLength(100)]
        public string EnteredBy { get; set; } = string.Empty;

        /// <summary>
        /// تاريخ المراجعة
        /// </summary>
        public DateTime? ReviewedDate { get; set; }

        /// <summary>
        /// اسم المراجع
        /// </summary>
        [StringLength(100)]
        public string ReviewedBy { get; set; } = string.Empty;

        /// <summary>
        /// هل تمت مراجعة النتيجة
        /// </summary>
        public bool IsReviewed { get; set; }

        /// <summary>
        /// هل تم الانتهاء من إدخال النتيجة
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// النتيجة
        /// </summary>
        public string Result => TextResult;
    }
}
