using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;
using OGRALAB.Enums;

namespace OGRALAB.Services
{
    /// <summary>
    /// تنفيذ خدمة إدارة أنواع التحاليل والمعدلات الطبيعية المرنة
    /// </summary>
    public class TestManagementService : ITestManagementService
    {
        private readonly OgralabDbContext _context;
        public TestManagementService(OgralabDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region إدارة التحاليل الأساسية
        public async Task<List<Test>> GetAllTestsAsync()
        {
            try
            {
                return await _context.Tests
                    .Include(t => t.ReferenceRanges)
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.TestName)
                    .ToListAsync();
            }
            catch
            {
                // ErrorLogger.Log(ex, "TestManagementService.GetAllTestsAsync");
                throw;
            }
        }

        public async Task<Test?> GetTestByIdAsync(int id)
        {
            try
            {
                return await _context.Tests
                    .Include(t => t.ReferenceRanges)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.GetTestByIdAsync({id})");
                throw;
            }
        }

        public async Task<bool> AddTestAsync(string name, string? unit, decimal price, string? description = null, string? category = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || price < 0)
                    return false;

                if (!await IsTestNameAvailableAsync(name))
                    return false;

                var test = new Test
                {
                    TestName = name.Trim(),
                    Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
                    Price = price,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };
                _context.Tests.Add(test);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.AddTestAsync({name})");
                return false;
            }
        }

        public async Task<bool> UpdateTestAsync(int id, string name, string? unit, decimal price, string? description = null, string? category = null)
        {
            try
            {
                var test = await GetTestByIdAsync(id);
                if (test == null)
                    return false;

                if (string.IsNullOrWhiteSpace(name) || price < 0)
                    return false;

                if (!await IsTestNameAvailableAsync(name, id))
                    return false;

                test.TestName = name.Trim();
                test.Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
                test.Price = price;
                test.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
                test.Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();

                _context.Tests.Update(test);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.UpdateTestAsync({id}, {name})");
                return false;
            }
        }

        public async Task<bool> DeleteTestAsync(int id)
        {
            try
            {
                if (!await CanDeleteTestAsync(id))
                    return false;

                var test = await GetTestByIdAsync(id);
                if (test == null)
                    return false;

                var referenceRanges = await _context.TestReferenceRanges
                    .Where(r => r.TestId == id)
                    .ToListAsync();
                if (referenceRanges.Any())
                {
                    _context.TestReferenceRanges.RemoveRange(referenceRanges);
                }

                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.DeleteTestAsync({id})");
                return false;
            }
        }

        public async Task<bool> IsTestNameAvailableAsync(string name, int? excludeTestId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return false;

                var query = _context.Tests.Where(t => t.TestName == name.Trim() && t.IsActive);

                if (excludeTestId.HasValue)
                {
                    query = query.Where(t => t.Id != excludeTestId.Value);
                }
                return !await query.AnyAsync();
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.IsTestNameAvailableAsync({name}, {excludeTestId})");
                return false;
            }
        }

        public async Task<bool> CanDeleteTestAsync(int testId)
        {
            try
            {
                var hasResults = await _context.TestResults.AnyAsync(tr => tr.TestId == testId);
                var hasRequests = await _context.TestRequests.AnyAsync(tr => tr.TestId == testId);
                return !hasResults && !hasRequests;
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.CanDeleteTestAsync({testId})");
                return false;
            }
        }
        #endregion

        #region إدارة المعدلات الطبيعية المرنة
        public async Task<List<TestReferenceRange>> GetTestReferenceRangesAsync(int testId)
        {
            try
            {
                return await _context.TestReferenceRanges
                    .Where(r => r.TestId == testId)
                    .OrderBy(r => r.Gender)
                    .ThenBy(r => r.AgeOperator)
                    .ThenBy(r => r.AgeValue1)
                    .ToListAsync();
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.GetTestReferenceRangesAsync({testId})");
                throw;
            }
        }

        public async Task<bool> AddReferenceRangeAsync(int testId, Gender gender, AgeOperator ageOperator,
            int ageValue1, int? ageValue2, string referenceValue, string? notes = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(referenceValue))
                    return false;

                if (!ValidateReferenceRangeData(ageOperator, ageValue1, ageValue2))
                    return false;

                var referenceRange = new TestReferenceRange
                {
                    TestId = testId,
                    Gender = gender,
                    AgeOperator = ageOperator,
                    AgeValue1 = ageValue1,
                    AgeValue2 = ageValue2,
                    ReferenceValue = referenceValue.Trim(),
                    Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim(),
                    CreatedAt = DateTime.Now
                };
                _context.TestReferenceRanges.Add(referenceRange);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.AddReferenceRangeAsync({testId}, {gender}, {ageOperator})");
                return false;
            }
        }

        public async Task<bool> UpdateReferenceRangeAsync(int id, Gender gender, AgeOperator ageOperator,
            int ageValue1, int? ageValue2, string referenceValue, string? notes = null)
        {
            try
            {
                var referenceRange = await _context.TestReferenceRanges.FindAsync(id);
                if (referenceRange == null)
                    return false;

                if (string.IsNullOrWhiteSpace(referenceValue))
                    return false;

                if (!ValidateReferenceRangeData(ageOperator, ageValue1, ageValue2))
                    return false;

                referenceRange.Gender = gender;
                referenceRange.AgeOperator = ageOperator;
                referenceRange.AgeValue1 = ageValue1;
                referenceRange.AgeValue2 = ageValue2;
                referenceRange.ReferenceValue = referenceValue.Trim();
                referenceRange.Notes = string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim();
                referenceRange.UpdatedAt = DateTime.Now;
                _context.TestReferenceRanges.Update(referenceRange);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.UpdateReferenceRangeAsync({id})");
                return false;
            }
        }

        public async Task<bool> DeleteReferenceRangeAsync(int id)
        {
            try
            {
                var referenceRange = await _context.TestReferenceRanges.FindAsync(id);
                if (referenceRange == null)
                    return false;

                _context.TestReferenceRanges.Remove(referenceRange);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.DeleteReferenceRangeAsync({id})");
                return false;
            }
        }

        public async Task<string> GetAppropriateReferenceRangeAsync(int testId, int age, Gender gender)
        {
            try
            {
                var referenceRanges = await _context.TestReferenceRanges
                    .Where(r => r.TestId == testId && r.Gender == gender)
                    .ToListAsync();

                var appropriateRange = referenceRanges
                    .FirstOrDefault(r => r.IsAgeInRange(age));

                if (appropriateRange != null)
                    return appropriateRange.ReferenceValue;

                var test = await _context.Tests.FindAsync(testId);
                if (test != null)
                {
                    if (gender == Gender.Male && !string.IsNullOrWhiteSpace(test.NormalRangeMale))
                        return test.NormalRangeMale;
                    if (gender == Gender.Female && !string.IsNullOrWhiteSpace(test.NormalRangeFemale))
                        return test.NormalRangeFemale;
                }

                return "غير محدد";
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.GetAppropriateReferenceRangeAsync({testId}, {age}, {gender})");
                return "غير محدد";
            }
        }

        public bool ValidateReferenceRangeData(AgeOperator ageOperator, int ageValue1, int? ageValue2)
        {
            if (ageValue1 < 0 || ageValue1 > 150)
                return false;

            if (ageOperator == AgeOperator.Range)
            {
                return ageValue2.HasValue && ageValue2.Value > ageValue1 && ageValue2.Value <= 150;
            }

            return true;
        }
        #endregion

        #region وظائف مساعدة
        public async Task<List<Test>> SearchTestsAsync(string searchText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchText))
                    return await GetAllTestsAsync();

                var searchTerm = searchText.Trim().ToLower();
                return await _context.Tests
                    .Include(t => t.ReferenceRanges)
                    .Where(t => t.IsActive &&
                           (t.TestName.ToLower().Contains(searchTerm) ||
                            (t.Category != null && t.Category.ToLower().Contains(searchTerm)) ||
                            (t.Description != null && t.Description.ToLower().Contains(searchTerm))))
                    .OrderBy(t => t.TestName)
                    .ToListAsync();
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.SearchTestsAsync({searchText})");
                return new List<Test>();
            }
        }

        public async Task<List<Test>> GetTestsByCategoryAsync(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                    return new List<Test>();

                return await _context.Tests
                    .Include(t => t.ReferenceRanges)
                    .Where(t => t.IsActive && t.Category == category.Trim())
                    .OrderBy(t => t.TestName)
                    .ToListAsync();
            }
            catch
            {
                // ErrorLogger.Log(ex, $"TestManagementService.GetTestsByCategoryAsync({category})");
                return new List<Test>();
            }
        }

        public async Task<List<string>> GetTestCategoriesAsync()
        {
            try
            {
                return await _context.Tests
                    .Where(t => t.IsActive && !string.IsNullOrEmpty(t.Category))
                    .Select(t => t.Category!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();
            }
            catch
            {
                // ErrorLogger.Log(ex, "TestManagementService.GetTestCategoriesAsync");
                return new List<string>();
            }
        }

        public async Task<(int TotalTests, int ActiveTests, int TestsWithReferenceRanges)> GetTestStatisticsAsync()
        {
            try
            {
                var totalTests = await _context.Tests.CountAsync();
                var activeTests = await _context.Tests.CountAsync(t => t.IsActive);
                var testsWithReferenceRanges = await _context.Tests
                    .Where(t => t.IsActive)
                    .CountAsync(t => t.ReferenceRanges.Any());

                return (totalTests, activeTests, testsWithReferenceRanges);
            }
            catch
            {
                // ErrorLogger.Log(ex, "TestManagementService.GetTestStatisticsAsync");
                return (0, 0, 0);
            }
        }
        #endregion
    }
}