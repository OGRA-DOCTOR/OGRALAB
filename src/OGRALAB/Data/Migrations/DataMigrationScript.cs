using OGRALAB.Data;
using OGRALAB.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace OGRALAB.Data.Migrations
{
    public static class DataMigrationScript
    {
        public static async Task MigrateOldReferenceRanges(ApplicationDbContext context)
        {
            // Get all tests that have old reference range data
            var testsWithOldRanges = await context.Tests
                .Where(t => t.NormalRangeMale != null || t.NormalRangeFemale != null || t.NormalRangeChildren != null)
                .ToListAsync();

            foreach (var test in testsWithOldRanges)
            {
                // Migrate Male ranges
                if (!string.IsNullOrEmpty(test.NormalRangeMale))
                {
                    // Assuming a simple case where NormalRangeMale might contain a single value or a range like "10-20"
                    // This parsing logic needs to be robust based on actual data format
                    // For simplicity, we'll just store the string in ReferenceValue for now
                    // NumericLow, NumericHigh, CriticalLow, CriticalHigh would need more sophisticated parsing
                    context.TestReferenceRanges.Add(new TestReferenceRange
                    {
                        TestId = test.Id,
                        Gender = Enums.Gender.Male,
                        AgeOperator = Enums.AgeOperator.GreaterThanOrEqual, // Defaulting for simplicity
                        AgeValue1 = 0, // Defaulting for simplicity
                        ReferenceValue = test.NormalRangeMale,
                        Notes = "Migrated from old NormalRangeMale field"
                    });
                }

                // Migrate Female ranges
                if (!string.IsNullOrEmpty(test.NormalRangeFemale))
                {
                    context.TestReferenceRanges.Add(new TestReferenceRange
                    {
                        TestId = test.Id,
                        Gender = Enums.Gender.Female,
                        AgeOperator = Enums.AgeOperator.GreaterThanOrEqual, // Defaulting for simplicity
                        AgeValue1 = 0, // Defaulting for simplicity
                        ReferenceValue = test.NormalRangeFemale,
                        Notes = "Migrated from old NormalRangeFemale field"
                    });
                }

                // Migrate Children ranges
                if (!string.IsNullOrEmpty(test.NormalRangeChildren))
                {
                    context.TestReferenceRanges.Add(new TestReferenceRange
                    {
                        TestId = test.Id,
                        Gender = Enums.Gender.Other, // Assuming 'Other' for children if no specific 'Child' enum
                        AgeOperator = Enums.AgeOperator.LessThanOrEqual, // Defaulting for simplicity
                        AgeValue1 = 18, // Assuming children are <18
                        ReferenceValue = test.NormalRangeChildren,
                        Notes = "Migrated from old NormalRangeChildren field"
                    });
                }

                // Also migrate MinNormalValue, MaxNormalValue, CriticalLowValue, CriticalHighValue
                // These would typically be associated with a default reference range if not already covered
                if (test.MinNormalValue.HasValue || test.MaxNormalValue.HasValue || test.CriticalLowValue.HasValue || test.CriticalHighValue.HasValue)
                {
                    // Create a generic reference range for these values if they don't fit into specific gender/age
                    // Or, ideally, this logic would be integrated into the above gender-specific migrations
                    // For now, creating a separate entry for simplicity
                    context.TestReferenceRanges.Add(new TestReferenceRange
                    {
                        TestId = test.Id,
                        Gender = Enums.Gender.Other, // Or a more appropriate default
                        AgeOperator = Enums.AgeOperator.GreaterThanOrEqual, // Default
                        AgeValue1 = 0, // Default
                        NumericLow = (double?)test.MinNormalValue,
                        NumericHigh = (double?)test.MaxNormalValue,
                        CriticalLow = (double?)test.CriticalLowValue,
                        CriticalHigh = (double?)test.CriticalHighValue,
                        ReferenceValue = "Migrated numeric values",
                        Notes = "Migrated from old numeric range fields"
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}


