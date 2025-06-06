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
    /// خدمة إدارة المرضى
    /// </summary>
    public class PatientService
    {
        private readonly OgralabDbContext _context;

        public PatientService(OgralabDbContext context)
        {
            // تم إعطاء قيمة لـ _context هنا، لذا لن يظهر التحذير CS8618 لهذا المُنشئ
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // تم إزالة المُنشئ الفارغ الذي كان يسبب التحذير CS8618
        // public PatientService()
        // {
        //     // Initialize service logic here
        // }

        public void PerformServiceLogic()
        {
            // Example method
            if (_context == null)
            {
                // Handle the case where _context might not be initialized if the parameterless constructor were kept and _context made nullable.
                // Since the parameterless constructor is removed, _context should always be initialized.
                throw new InvalidOperationException("Database context is not initialized.");
            }
            // Add logic here
        }

        /// <summary>
        /// إنشاء كود مريض جديد
        /// </summary>
        /// <returns>كود المريض الجديد</returns>
        public async Task<string> GeneratePatientCodeAsync()
        {
            var today = DateTime.Now;
            var datePrefix = today.ToString("yyyyMMdd");

            // البحث عن آخر رقم تسلسلي لليوم الحالي
            var lastPatientToday = await _context.Patients
                .Where(p => p.PatientCode.StartsWith(datePrefix))
                .OrderByDescending(p => p.PatientCode)
                .FirstOrDefaultAsync();

            int dailySequence = 1;
            if (lastPatientToday != null)
            {
                // التأكد من أن طول PatientCode يسمح باستخلاص الجزء الرقمي بشكل صحيح
                if (lastPatientToday.PatientCode.Length > 8)
                {
                    var lastSequenceStr = lastPatientToday.PatientCode.Substring(8);
                    if (int.TryParse(lastSequenceStr, out int lastSequence))
                    {
                        dailySequence = lastSequence + 1;
                    }
                }
            }

            return $"{datePrefix}{dailySequence:D4}";
        }

        /// <summary>
        /// إضافة مريض جديد
        /// </summary>
        /// <param name="patient">بيانات المريض</param>
        /// <returns>المريض المُضاف</returns>
        public async Task<Patient> AddPatientAsync(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            // إنشاء كود المريض إذا لم يكن موجوداً
            if (string.IsNullOrEmpty(patient.PatientCode))
            {
                patient.PatientCode = await GeneratePatientCodeAsync();
            }

            patient.CreatedDate = DateTime.Now;

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return patient;
        }

        /// <summary>
        /// الحصول على جميع المرضى
        /// </summary>
        /// <returns>قائمة المرضى</returns>
        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.Entity)
                .Include(p => p.TestRequests)
                    .ThenInclude(tr => tr.Test)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// الحصول على المرضى لليوم الحالي
        /// </summary>
        /// <returns>قائمة مرضى اليوم</returns>
        public async Task<List<Patient>> GetTodayPatientsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.Entity)
                .Include(p => p.TestRequests)
                    .ThenInclude(tr => tr.Test)
                .Where(p => p.CreatedDate >= today && p.CreatedDate < tomorrow)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// البحث عن مريض بالكود
        /// </summary>
        /// <param name="patientCode">كود المريض</param>
        /// <returns>المريض إذا تم العثور عليه</returns>
        public async Task<Patient?> GetPatientByCodeAsync(string patientCode)
        {
            if (string.IsNullOrWhiteSpace(patientCode)) return null;

            return await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.Entity)
                .Include(p => p.TestRequests)
                    .ThenInclude(tr => tr.Test)
                .Include(p => p.TestRequests)
                    .ThenInclude(tr => tr.TestResult)
                .FirstOrDefaultAsync(p => p.PatientCode == patientCode);
        }

        /// <summary>
        /// البحث عن المرضى
        /// </summary>
        /// <param name="searchText">النص المراد البحث عنه</param>
        /// <returns>قائمة المرضى المطابقة</returns>
        public async Task<List<Patient>> SearchPatientsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllPatientsAsync();

            searchText = searchText.Trim().ToLower();

            return await _context.Patients
                .Include(p => p.Doctor)
                .Include(p => p.Entity)
                .Include(p => p.TestRequests)
                    .ThenInclude(tr => tr.Test)
                .Where(p =>
                    p.PatientCode.ToLower().Contains(searchText) ||
                    p.FullName.ToLower().Contains(searchText) ||
                    (p.MobileNumber != null && p.MobileNumber.Contains(searchText)) || // Add null check for MobileNumber
                    (p.Doctor != null && p.Doctor.FullName.ToLower().Contains(searchText)))
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// تحديث بيانات مريض
        /// </summary>
        /// <param name="patient">بيانات المريض المُحدثة</param>
        /// <returns>المريض المُحدث</returns>
        public async Task<Patient> UpdatePatientAsync(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            patient.LastModifiedDate = DateTime.Now;

            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            return patient;
        }

        /// <summary>
        /// حذف مريض
        /// </summary>
        /// <param name="patientId">معرف المريض</param>
        /// <returns>true إذا تم الحذف بنجاح</returns>
        public async Task<bool> DeletePatientAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
                return false;

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// التحقق من صحة رقم الموبايل
        /// </summary>
        /// <param name="mobileNumber">رقم الموبايل</param>
        /// <returns>true إذا كان الرقم صحيح</returns>
        public bool ValidateMobileNumber(string? mobileNumber) // Made mobileNumber nullable to match potential usage
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                return false;

            // التحقق من أن الرقم يحتوي على 11 رقم بالضبط وأن جميعها أرقام
            return mobileNumber.Length == 11 && mobileNumber.All(char.IsDigit);
        }
    }
}