using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OGRALAB.Enums;

namespace OGRALAB.Models
{
    public class TestReferenceRange
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TestId { get; set; }

        [ForeignKey("TestId")]
        public virtual Test Test { get; set; } = null!;

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public AgeOperator AgeOperator { get; set; }

        [Required]
        public int AgeValue1 { get; set; }

        public int? AgeValue2 { get; set; }

        [Required]
        [StringLength(200)]
        public string ReferenceValue { get; set; } = string.Empty;

        public double? NumericLow { get; set; }
        public double? NumericHigh { get; set; }
        public double? CriticalLow { get; set; }
        public double? CriticalHigh { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public string AgeRangeDescription
        {
            get
            {
                return AgeOperator switch
                {
                    AgeOperator.Range => $"من {AgeValue1} إلى {AgeValue2} سنة",
                    AgeOperator.GreaterThan => $"أكبر من {AgeValue1} سنة",
                    AgeOperator.LessThan => $"أقل من {AgeValue1} سنة",
                    AgeOperator.Equals => $"{AgeValue1} سنة",
                    AgeOperator.GreaterThanOrEqual => $"{AgeValue1} سنة فأكثر",
                    AgeOperator.LessThanOrEqual => $"{AgeValue1} سنة فأقل",
                    _ => "غير محدد"
                };
            }
        }

        // --- تمت إضافة هذه الخاصية لتتوافق مع الـ ViewModel ---
        [NotMapped]
        public string FullDescription
        {
            get
            {
                string genderText = Gender == Gender.Male ? "ذكر" : "أنثى";
                return $"{genderText}, {AgeRangeDescription}: {ReferenceValue}";
            }
        }

        public bool IsAgeInRange(int age)
        {
            return AgeOperator switch
            {
                AgeOperator.Range => age >= AgeValue1 && age <= (AgeValue2 ?? AgeValue1),
                AgeOperator.GreaterThan => age > AgeValue1,
                AgeOperator.LessThan => age < AgeValue1,
                AgeOperator.Equals => age == AgeValue1,
                AgeOperator.GreaterThanOrEqual => age >= AgeValue1,
                AgeOperator.LessThanOrEqual => age <= AgeValue1,
                _ => false
            };
        }
    }
}