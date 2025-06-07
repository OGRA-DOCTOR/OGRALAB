using OGRALAB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
namespace OGRALAB.Services
{
    /// <summary>
    /// خدمة إنشاء وإدارة التقارير الطبية
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// جلب قائمة المرضى المتاحين للتقارير (الذين لديهم فحوصات)
        /// </summary>
        Task<List<PatientReportData>> GetPatientsForReportsAsync();

        /// <summary>
        /// جلب بيانات تقرير مريض محدد مع جميع فحوصاته
        /// </summary>
        Task<PatientReportData> GetPatientReportDataAsync(int patientId);

        /// <summary>
        /// إنشاء مستند التقرير للطباعة والمعاينة
        /// </summary>
        FlowDocument CreateReportDocument(PreparedReportData reportData);

        /// <summary>
        /// جلب قالب التقرير الافتراضي (قابل للتخصيص في المرحلة 5)
        /// </summary>
        ReportTemplate GetDefaultReportTemplate();

        /// <summary>
        /// فحص ما إذا كانت النتيجة غير طبيعية بناءً على المدى المرجعي
        /// </summary>
        bool IsResultAbnormal(string result, string referenceRange);

        /// <summary>
        /// تنسيق نتيجة الفحص للعرض
        /// </summary>
        string FormatTestResult(TestReportItem test);

        /// <summary>
        /// إعداد بيانات التقرير للطباعة
        /// </summary>
        PreparedReportData PrepareReportData(PatientReportData patientData, List<TestReportItem> selectedTests);
    }
}