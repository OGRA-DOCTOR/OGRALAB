using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OGRALAB.Models
{
    /// <summary>
    /// نموذج بيانات الطبيب
    /// </summary>
    public class Doctor
    {
        /// <summary>
        /// المعرف الفريد للطبيب
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// اسم الطبيب الكامل
        /// </summary>
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// التخصص
        /// </summary>
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        /// <summary>
        /// رقم الهاتف
        /// </summary>
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// البريد الإلكتروني
        /// </summary>
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// العنوان
        /// </summary>
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// ملاحظات
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// حالة النشاط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاريخ الإنشاء
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// قائمة المرضى المرتبطين بالطبيب
        /// </summary>
        public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

        /// <summary>
        /// اسم الطبيب
        /// </summary>
        public string Name => FullName;
    }
}
