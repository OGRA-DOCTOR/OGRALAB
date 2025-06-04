using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OGRALAB.Models
{
    /// <summary>
    /// نموذج بيانات الجهة/العيادة/المستشفى
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// المعرف الفريد للجهة
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// اسم الجهة
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// نوع الجهة (عيادة، مستشفى، مركز طبي، إلخ)
        /// </summary>
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// عنوان الجهة
        /// </summary>
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// اسم المسؤول
        /// </summary>
        [StringLength(200)]
        public string ResponsiblePerson { get; set; } = string.Empty;

        /// <summary>
        /// رقم الهاتف
        /// </summary>
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// رقم الفاكس
        /// </summary>
        [StringLength(20)]
        public string FaxNumber { get; set; } = string.Empty;

        /// <summary>
        /// البريد الإلكتروني
        /// </summary>
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

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
        /// قائمة المرضى المرتبطين بالجهة
        /// </summary>
        public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
    }
}
