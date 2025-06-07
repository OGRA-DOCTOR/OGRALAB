using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;

namespace OGRALAB.Services
{
    /// <summary>
    /// خدمة إدارة الفحوصات
    /// </summary>
    public class TestService
    {
        private readonly OgralabDbContext _context;

        public TestService(OgralabDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// الحصول على جميع الفحوصات النشطة
        /// </summary>
        /// <returns>قائمة الفحوصات</returns>
        public async Task<List<Test>> GetActiveTestsAsync()
        {
            return await _context.Tests
                .Where(t => t.IsActive)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.DisplayOrder)
                .ThenBy(t => t.TestName)
                .ToListAsync();
        }

        /// <summary>
        /// الحصول على الفحوصات مجمعة حسب التخصص
        /// </summary>
        /// <returns>الفحوصات مجمعة حسب التخصص</returns>
        public async Task<Dictionary<string, List<Test>>> GetTestsByCategoryAsync()
        {
            var tests = await GetActiveTestsAsync();

            return tests
                .GroupBy(t => string.IsNullOrEmpty(t.Category) ? "عام" : t.Category!)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// البحث في الفحوصات
        /// </summary>
        /// <param name="searchText">النص المراد البحث عنه</param>
        /// <returns>قائمة الفحوصات المطابقة</returns>
        public async Task<List<Test>> SearchTestsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetActiveTestsAsync();

            searchText = searchText.Trim().ToLower();

            return await _context.Tests
                .Where(t => t.IsActive && (
                    t.TestName.ToLower().Contains(searchText) ||
                    (t.TestCode != null && t.TestCode.ToLower().Contains(searchText)) ||
                    (t.Abbreviation != null && t.Abbreviation.ToLower().Contains(searchText)) ||
                    (t.Category != null && t.Category.ToLower().Contains(searchText))))
                .OrderBy(t => t.Category)
                .ThenBy(t => t.TestName)
                .ToListAsync();
        }

        /// <summary>
        /// الحصول على فحص بالمعرف
        /// </summary>
        /// <param name="testId">معرف الفحص</param>
        /// <returns>الفحص إذا تم العثور عليه</returns>
        public async Task<Test?> GetTestByIdAsync(int testId)
        {
            return await _context.Tests.FindAsync(testId);
        }

        /// <summary>
        /// إضافة طلب فحص جديد
        /// </summary>
        /// <param name="testRequest">طلب الفحص</param>
        /// <returns>طلب الفحص المُضاف</returns>
        public async Task<TestRequest> AddTestRequestAsync(TestRequest testRequest)
        {
            testRequest.RequestedDate = DateTime.Now;
            testRequest.Status = "Requested";

            _context.TestRequests.Add(testRequest);
            await _context.SaveChangesAsync();

            return testRequest;
        }

        /// <summary>
        /// إضافة طلبات فحوصات متعددة
        /// </summary>
        /// <param name="testRequests">قائمة طلبات الفحوصات</param>
        /// <returns>طلبات الفحوصات المُضافة</returns>
        public async Task<List<TestRequest>> AddTestRequestsAsync(List<TestRequest> testRequests)
        {
            foreach (var request in testRequests)
            {
                request.RequestedDate = DateTime.Now;
                request.Status = "Requested";
            }

            _context.TestRequests.AddRange(testRequests);
            await _context.SaveChangesAsync();

            return testRequests;
        }

        /// <summary>
        /// الحصول على طلبات الفحوصات لمريض معين
        /// </summary>
        /// <param name="patientId">معرف المريض</param>
        /// <returns>قائمة طلبات الفحوصات</returns>
        public async Task<List<TestRequest>> GetTestRequestsByPatientAsync(int patientId)
        {
            return await _context.TestRequests
                .Include(tr => tr.Test)
                .Include(tr => tr.TestResult)
                .Where(tr => tr.PatientId == patientId)
                .OrderBy(tr => tr.Test!.Category) // تم إضافة ! هنا
                .ThenBy(tr => tr.Test!.DisplayOrder) // تم إضافة ! هنا
                .ToListAsync();
        }

        /// <summary>
        /// حساب التكلفة الإجمالية للفحوصات
        /// </summary>
        /// <param name="testIds">معرفات الفحوصات</param>
        /// <returns>التكلفة الإجمالية</returns>
        public async Task<decimal> CalculateTotalCostAsync(List<int> testIds)
        {
            return await _context.Tests
                .Where(t => testIds.Contains(t.Id))
                .SumAsync(t => t.Price);
        }

        /// <summary>
        /// حساب قيمة الخصم
        /// </summary>
        /// <param name="totalAmount">المبلغ الإجمالي</param>
        /// <param name="discountPercentage">نسبة الخصم</param>
        /// <returns>قيمة الخصم</returns>
        public decimal CalculateDiscountAmount(decimal totalAmount, decimal discountPercentage)
        {
            return totalAmount * discountPercentage / 100;
        }

        /// <summary>
        /// حساب المبلغ بعد الخصم
        /// </summary>
        /// <param name="totalAmount">المبلغ الإجمالي</param>
        /// <param name="discountAmount">قيمة الخصم</param>
        /// <returns>المبلغ بعد الخصم</returns>
        public decimal CalculateAmountAfterDiscount(decimal totalAmount, decimal discountAmount)
        {
            return totalAmount - discountAmount;
        }

        /// <summary>
        /// حساب المتبقي للتحصيل
        /// </summary>
        /// <param name="amountAfterDiscount">المبلغ بعد الخصم</param>
        /// <param name="paidAmount">المدفوع مسبقاً</param>
        /// <returns>المتبقي للتحصيل</returns>
        public decimal CalculateRemainingAmount(decimal amountAfterDiscount, decimal paidAmount)
        {
            return amountAfterDiscount - paidAmount;
        }
    }
}