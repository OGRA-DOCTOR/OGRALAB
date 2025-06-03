using System;
using System.ComponentModel.DataAnnotations;

namespace OGRALAB.Models
{
    public class UserSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        public DateTime? RememberMeExpiry { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
