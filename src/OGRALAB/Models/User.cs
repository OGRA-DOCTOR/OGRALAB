using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OGRALAB.Enums;

namespace OGRALAB.Models
{
    /// <summary>
    /// نموذج بيانات المستخدم - محدث للمرحلة الخامسة
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        // --- خصائص محسوبة ---
        [NotMapped]
        public string RoleDescription => GetRoleDisplayName();

        [NotMapped]
        public bool IsAdmin => Role == UserRole.Admin;

        [NotMapped]
        public string DisplayName => !string.IsNullOrWhiteSpace(FullName) ? FullName : Username;

        // --- دوال مساعدة ---
        private string GetRoleDisplayName()
        {
            return Role switch
            {
                UserRole.Admin => "مسؤول",
                UserRole.User => "مستخدم عادي",
                _ => "غير محدد"
            };
        }

        /// <summary>
        /// تحديث وقت آخر تعديل للبيانات
        /// هذه هي الدالة التي تمت إضافتها لحل الخطأ
        /// </summary>
        public void UpdateInfo()
        {
            UpdatedAt = DateTime.Now;
        }
    }
}