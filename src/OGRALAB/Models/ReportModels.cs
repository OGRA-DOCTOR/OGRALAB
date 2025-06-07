using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
namespace OGRALAB.Models
{
    /// <summary>
    /// نموذج بيانات تقرير المريض
    /// </summary>
    public class PatientReportData
    {
        public int PatientId { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public List<TestReportItem> AvailableTests { get; set; } = new();

        public string DisplayText => $"{FullName} - {PatientCode} ({RegistrationDate:yyyy/MM/dd})";
        public int CompletedTestsCount => AvailableTests.Count(t => t.HasResult);
        public int TotalTestsCount => AvailableTests.Count;
    }
    /// <summary>
    /// عنصر فحص في التقرير
    /// </summary>
    public class TestReportItem : INotifyPropertyChanged
    {
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestResult { get; set; } = string.Empty;
        public string ReferenceRange { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public bool HasResult { get; set; }
        public bool IsAbnormal { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public bool CanBeSelected => HasResult;
        public string StatusText => HasResult ? "✅ مكتمل" : "⏳ معلق";
        public string FormattedResult => string.IsNullOrEmpty(Unit) ? TestResult : $"{TestResult} {Unit}";
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /// <summary>
    /// قالب التقرير القابل للتخصيص (للمرحلة 5)
    /// </summary>
    public class ReportTemplate
    {
        // بيانات افتراضية ستكون قابلة للتخصيص في المرحلة 5
        public string LabName { get; set; } = "OGRALAB Medical Laboratory";
        public string LabAddress { get; set; } = string.Empty; // سيتم تعبئتها في المرحلة 5
        public string LabPhone { get; set; } = string.Empty; // سيتم تعبئتها في المرحلة 5
        public string LabEmail { get; set; } = string.Empty; // سيتم تعبئتها في المرحلة 5
        public string LabLicense { get; set; } = string.Empty; // سيتم تعبئتها في المرحلة 5
        public string LogoPath { get; set; } = string.Empty; // سيتم تعبئتها في المرحلة 5
        public string ReportTitle { get; set; } = "تقرير نتائج التحاليل الطبية";
        public string FooterText { get; set; } = string.Empty; // سيتم تعبئتها في المرحلة 5

        // إعدادات التخطيط (قابلة للتخصيص)
        public bool ShowLogo { get; set; } = false; // مُعطل مؤقتاً حتى المرحلة 5
        public bool ShowLabInfo { get; set; } = true;
        public bool ShowReferenceRanges { get; set; } = true;
        public bool HighlightAbnormalResults { get; set; } = true;

        // ألوان التقرير (قابلة للتخصيص)
        public string HeaderColor { get; set; } = "#2C5282";
        public string AbnormalResultColor { get; set; } = "#E53E3E";
        public string NormalResultColor { get; set; } = "#2D3748";
    }
    /// <summary>
    /// بيانات التقرير المُجهز للطباعة
    /// </summary>
    public class PreparedReportData
    {
        public PatientReportData Patient { get; set; } = new();
        public List<TestReportItem> SelectedTests { get; set; } = new();
        public ReportTemplate Template { get; set; } = new();
        public DateTime GeneratedOn { get; set; } = DateTime.Now;
        public string GeneratedBy { get; set; } = "OGRALAB System";
    }
}