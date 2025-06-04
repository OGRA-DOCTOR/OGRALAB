using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OGRALAB.Models
{
    /// <summary>
    /// نموذج بيانات الفحص المخبري
    /// </summary>
    public class Test
    {
        /// <summary>
        /// المعرف الفريد للفحص
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// كود الفحص
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TestCode { get; set; } = string.Empty;

        /// <summary>
        /// اسم الفحص
        /// </summary>
        [Required]
        [StringLength(200)]
        public string TestName { get; set; } = string.Empty;

        /// <summary>
        /// اختصار الفحص
        /// </summary>
        [StringLength(50)]
        public string Abbreviation { get; set; } = string.Empty;

        /// <summary>
        /// التخصص أو القسم
        /// </summary>
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// نوع العينة المطلوبة
        /// </summary>
        [StringLength(100)]
        public string SampleType { get; set; } = string.Empty;

        /// <summary>
        /// الوحدة
        /// </summary>
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// المعدل الطبيعي (للذكور)
        /// </summary>
        [StringLength(100)]
        public string NormalRangeMale { get; set; } = string.Empty;

        /// <summary>
        /// المعدل الطبيعي (للإناث)
        /// </summary>
        [StringLength(100)]
        public string NormalRangeFemale { get; set; } = string.Empty;

        /// <summary>
        /// المعدل الطبيعي (للأطفال)
        /// </summary>
        [StringLength(100)]
        public string NormalRangeChildren { get; set; } = string.Empty;

        /// <summary>
        /// الحد الأدنى للقيم الطبيعية
        /// </summary>
        public decimal? MinNormalValue { get; set; }

        /// <summary>
        /// الحد الأقصى للقيم الطبيعية
        /// </summary>
        public decimal? MaxNormalValue { get; set; }

        /// <summary>
        /// الحد الحرج الأدنى
        /// </summary>
        public decimal? CriticalLowValue { get; set; }

        /// <summary>
        /// الحد الحرج الأعلى
        /// </summary>
        public decimal? CriticalHighValue { get; set; }

        /// <summary>
        /// سعر الفحص
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// وصف الفحص
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// التحضيرات المطلوبة قبل الفحص
        /// </summary>
        [StringLength(500)]
        public string Preparation { get; set; } = string.Empty;

        /// <summary>
        /// حالة النشاط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ترتيب العرض
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// تاريخ الإنشاء
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// قائمة طلبات الفحوصات
        /// </summary>
        public virtual ICollection<TestRequest> TestRequests { get; set; } = new List<TestRequest>();

        /// <summary>
        /// قائمة نتائج الفحوصات
        /// </summary>
        public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
