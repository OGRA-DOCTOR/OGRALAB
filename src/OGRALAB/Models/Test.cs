using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OGRALAB.Models;

namespace OGRALAB.Models
{
    public class Test
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string TestCode { get; set; } = string.Empty;

        // --- تم تغيير الاسم من TestName ليتوافق مع الـ ViewModel ---
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Abbreviation { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? SampleType { get; set; }

        [StringLength(20)]
        public string? Unit { get; set; }

        [StringLength(100)]
        public string? NormalRangeMale { get; set; }

        [StringLength(100)]
        public string? NormalRangeFemale { get; set; }

        [StringLength(100)]
        public string? NormalRangeChildren { get; set; }

        public decimal? MinNormalValue { get; set; }
        public decimal? MaxNormalValue { get; set; }
        public decimal? CriticalLowValue { get; set; }
        public decimal? CriticalHighValue { get; set; }

        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Preparation { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<TestRequest> TestRequests { get; set; } = new List<TestRequest>();
        public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
        public virtual ICollection<TestReferenceRange> ReferenceRanges { get; set; } = new List<TestReferenceRange>();
    }
}