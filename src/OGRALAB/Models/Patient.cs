using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OGRALAB.Enums;

namespace OGRALAB.Models
{
    /// <summary>
    /// نموذج بيانات المريض
    /// </summary>
    public class Patient
    {
        /// <summary>
        /// المعرف الفريد للمريض
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// كود المريض (YYYYMMDD + رقم تسلسلي يومي)
        /// </summary>
        [Required]
        [StringLength(12)]
        public string PatientCode { get; set; } = string.Empty;

        /// <summary>
        /// اللقب
        /// </summary>
        [StringLength(20)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// اسم المريض الكامل
        /// </summary>
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// جنس المريض
        /// </summary>
        public Gender Gender { get; set; } = Gender.Unknown;

        /// <summary>
        /// العمر
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// وحدة العمر
        /// </summary>
        public AgeUnit AgeUnit { get; set; } = AgeUnit.Years;

        /// <summary>
        /// رقم الموبايل (11 رقم)
        /// </summary>
        [StringLength(11)]
        public string MobileNumber { get; set; } = string.Empty;

        /// <summary>
        /// معرف الطبيب
        /// </summary>
        public int? DoctorId { get; set; }

        /// <summary>
        /// الطبيب المرتبط
        /// </summary>
        public virtual Doctor? Doctor { get; set; }

        /// <summary>
        /// معرف الجهة/العيادة
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// الجهة/العيادة المرتبطة
        /// </summary>
        public virtual Entity? Entity { get; set; }

        /// <summary>
        /// إجمالي المبلغ
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// نسبة الخصم (%)
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// قيمة الخصم
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// المبلغ بعد الخصم
        /// </summary>
        public decimal AmountAfterDiscount { get; set; }

        /// <summary>
        /// المدفوع مسبقاً
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// المتبقي للتحصيل
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// هل يتم طباعة الفاتورة
        /// </summary>
        public bool PrintInvoice { get; set; }

        /// <summary>
        /// تاريخ الإنشاء
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// تاريخ آخر تحديث
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// قائمة طلبات الفحوصات للمريض
        /// </summary>
        public virtual ICollection<TestRequest> TestRequests { get; set; } = new List<TestRequest>();
    }
}
