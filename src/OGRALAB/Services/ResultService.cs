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
    /// خدمة إدارة نتائج الفحوصات
    /// </summary>
    public class ResultService
    {
        private readonly OgralabDbContext _context;

        public ResultService(OgralabDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// إضافة أو تحديث نتيجة فحص
        /// </summary>
        /// <param name="testResult">نتيجة الفحص</param>
        /// <param name="enteredBy">اسم المستخدم الذي أدخل النتيجة</param>
        /// <returns>نتيجة الفحص المُضافة/المُحدثة</returns>
        public async Task<TestResult> SaveTestResultAsync(TestResult testResult, string enteredBy)
        {
            var existingResult = await _context.TestResults
                .FirstOrDefaultAsync(tr => tr.TestRequestId == testResult.TestRequestId);

            if (existingResult != null)
            {
                // تحديث النتيجة الموجودة
                existingResult.TextResult = testResult.TextResult;
                existingResult.NumericResult = testResult.NumericResult;
                existingResult.Unit = testResult.Unit;
                existingResult.Comments = testResult.Comments;
                existingResult.ResultDate = DateTime.Now;
                existingResult.EnteredBy = enteredBy;
                existingResult.IsCompleted = testResult.IsCompleted;

                // إعادة حساب العلامة والمعدل الطبيعي
                await UpdateTestFlagAndNormalRangeAsync(existingResult);

                _context.TestResults.Update(existingResult);
                testResult = existingResult;
            }
            else
            {
                // إضافة نتيجة جديدة
                testResult.ResultDate = DateTime.Now;
                testResult.EnteredBy = enteredBy;

                // حساب العلامة والمعدل الطبيعي
                await UpdateTestFlagAndNormalRangeAsync(testResult);

                _context.TestResults.Add(testResult);
            }

            // تحديث حالة طلب الفحص
            var testRequest = await _context.TestRequests.FindAsync(testResult.TestRequestId);
            if (testRequest != null)
            {
                testRequest.IsCompleted = testResult.IsCompleted;
                if (testResult.IsCompleted)
                {
                    testRequest.CompletedDate = DateTime.Now;
                    testRequest.Status = "Completed";
                }
                else
                {
                    testRequest.Status = "In Progress";
                }

                _context.TestRequests.Update(testRequest);
            }

            await _context.SaveChangesAsync();
            return testResult;
        }

        /// <summary>
        /// تحديث علامة الفحص والمعدل الطبيعي المطبق
        /// </summary>
        /// <param name="testResult">نتيجة الفحص</param>
        private async Task UpdateTestFlagAndNormalRangeAsync(TestResult testResult)
        {
            var test = await _context.Tests.FindAsync(testResult.TestId);
            if (test == null) return;

            var testRequest = await _context.TestRequests
                .Include(tr => tr.Patient)
                .FirstOrDefaultAsync(tr => tr.Id == testResult.TestRequestId);

            if (testRequest?.Patient == null) return;

            var patient = testRequest.Patient;

            // تحديد المعدل الطبيعي المناسب
            string normalRange = "";
            decimal? minNormal = test.MinNormalValue;
            decimal? maxNormal = test.MaxNormalValue;

            if (patient.AgeUnit == AgeUnit.Years && patient.Age < 18)
            {
                normalRange = test.NormalRangeChildren ?? "";
            }
            else if (patient.Gender == Gender.Male)
            {
                normalRange = test.NormalRangeMale ?? "";
            }
            else if (patient.Gender == Gender.Female)
            {
                normalRange = test.NormalRangeFemale ?? "";
            }
            else
            {
                normalRange = test.NormalRangeMale ?? "";
            }

            testResult.AppliedNormalRange = normalRange;
            testResult.Unit = test.Unit ?? "";

            // تحديد علامة الفحص إذا كانت النتيجة رقمية
            if (testResult.NumericResult.HasValue && minNormal.HasValue && maxNormal.HasValue)
            {
                var value = testResult.NumericResult.Value;

                // فحص القيم الحرجة أولاً
                if ((test.CriticalLowValue.HasValue && value <= test.CriticalLowValue.Value) ||
                    (test.CriticalHighValue.HasValue && value >= test.CriticalHighValue.Value))
                {
                    testResult.Flag = TestFlag.Critical;
                }
                else if (value < minNormal.Value)
                {
                    testResult.Flag = TestFlag.Low;
                }
                else if (value > maxNormal.Value)
                {
                    testResult.Flag = TestFlag.High;
                }
                else
                {
                    testResult.Flag = TestFlag.Normal;
                }
            }
            else
            {
                // للنتائج النصية، اعتبرها طبيعية افتراضياً
                testResult.Flag = TestFlag.Normal;
            }
        }

        /// <summary>
        /// الحصول على نتائج فحوصات مريض معين
        /// </summary>
        /// <param name="patientId">معرف المريض</param>
        /// <returns>قائمة نتائج الفحوصات</returns>
        public async Task<List<TestResult>> GetTestResultsByPatientAsync(int patientId)
        {
            return await _context.TestResults
                .Include(tr => tr.Test)
                .Include(tr => tr.TestRequest)
                    .ThenInclude(req => req.Patient)
                .Where(tr => tr.TestRequest!.PatientId == patientId) // تم إضافة ! هنا
                .OrderBy(tr => tr.Test!.Category) // تم إضافة ! هنا
                .ThenBy(tr => tr.Test!.DisplayOrder) // تم إضافة ! هنا
                .ToListAsync();
        }

        /// <summary>
        /// الحصول على نتيجة فحص بمعرف طلب الفحص
        /// </summary>
        /// <param name="testRequestId">معرف طلب الفحص</param>
        /// <returns>نتيجة الفحص إذا توجد</returns>
        public async Task<TestResult?> GetTestResultByRequestAsync(int testRequestId)
        {
            return await _context.TestResults
                .Include(tr => tr.Test)
                .Include(tr => tr.TestRequest)
                    .ThenInclude(req => req.Patient)
                .FirstOrDefaultAsync(tr => tr.TestRequestId == testRequestId);
        }

        /// <summary>
        /// مراجعة نتيجة فحص
        /// </summary>
        /// <param name="testResultId">معرف نتيجة الفحص</param>
        /// <param name="reviewedBy">اسم المراجع</param>
        /// <returns>true إذا تم التحديث بنجاح</returns>
        public async Task<bool> ReviewTestResultAsync(int testResultId, string reviewedBy)
        {
            var testResult = await _context.TestResults.FindAsync(testResultId);
            if (testResult == null) return false;

            testResult.IsReviewed = true;
            testResult.ReviewedDate = DateTime.Now;
            testResult.ReviewedBy = reviewedBy;

            // تحديث طلب الفحص أيضاً
            var testRequest = await _context.TestRequests.FindAsync(testResult.TestRequestId);
            if (testRequest != null)
            {
                testRequest.IsReviewed = true;
                _context.TestRequests.Update(testRequest);
            }

            _context.TestResults.Update(testResult);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// تحديد حالة طباعة النتيجة
        /// </summary>
        /// <param name="testRequestId">معرف طلب الفحص</param>
        /// <param name="isPrinted">هل تمت الطباعة</param>
        /// <returns>true إذا تم التحديث بنجاح</returns>
        public async Task<bool> UpdatePrintStatusAsync(int testRequestId, bool isPrinted)
        {
            var testRequest = await _context.TestRequests.FindAsync(testRequestId);
            if (testRequest == null) return false;

            testRequest.IsPrinted = isPrinted;
            _context.TestRequests.Update(testRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// تحديد حالة تصدير النتيجة
        /// </summary>
        /// <param name="testRequestId">معرف طلب الفحص</param>
        /// <param name="isExported">هل تم التصدير</param>
        /// <returns>true إذا تم التحديث بنجاح</returns>
        public async Task<bool> UpdateExportStatusAsync(int testRequestId, bool isExported)
        {
            var testRequest = await _context.TestRequests.FindAsync(testRequestId);
            if (testRequest == null) return false;

            testRequest.IsExported = isExported;
            _context.TestRequests.Update(testRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// الحصول على التعليقات الجاهزة
        /// </summary>
        /// <returns>قائمة التعليقات الجاهزة</returns>
        public List<string> GetPredefinedComments()
        {
            return new List<string>
            {
                "النتيجة طبيعية",
                "يُنصح بإعادة الفحص بعد شهر",
                "يُنصح بمراجعة الطبيب المختص",
                "النتيجة تحتاج لمتابعة دورية",
                "يُنصح بالصيام 12 ساعة قبل الفحص القادم",
                "النتيجة ضمن الحدود الطبيعية للعمر",
                "قد تتأثر النتيجة بالأدوية المتناولة",
                "يُنصح بتكرار الفحص للتأكيد",
                "النتيجة تحتاج لفحوصات إضافية",
                "يُرجى مراجعة الطبيب فوراً"
            };
        }
    }
}