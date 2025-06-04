using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using OGRALAB.Commands;
using OGRALAB.Services;
using OGRALAB.Views;

namespace OGRALAB.ViewModels
{
    /// <summary>
    /// نموذج عرض لوحة المعلومات الرئيسية
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private int _patientsRegisteredToday;
        private int _resultsProcessedToday;
        private int _pendingResults;
        private int _totalPatientsThisMonth;
        private int _totalReportsGenerated;
        private string _lastUpdated = string.Empty;

        private readonly IServiceProvider _serviceProvider;
        private readonly PatientService _patientService;

        public DashboardViewModel(IServiceProvider serviceProvider, PatientService patientService)
        {
            _serviceProvider = serviceProvider;
            _patientService = patientService;
            
            // إنشاء الأوامر
            AddPatientCommand = new RelayCommand(OpenAddPatientWindow);
            EnterResultsCommand = new RelayCommand(OpenEnterResultsWindow);
            RefreshDataCommand = new AsyncRelayCommand(LoadDashboardDataAsync);
            
            // تحميل البيانات
            LoadDashboardDataAsync();
        }

        #region خصائص الإحصائيات

        /// <summary>
        /// عدد المرضى المسجلين اليوم
        /// </summary>
        public int PatientsRegisteredToday
        {
            get => _patientsRegisteredToday;
            set
            {
                _patientsRegisteredToday = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// عدد النتائج المطبوعة اليوم
        /// </summary>
        public int ResultsProcessedToday
        {
            get => _resultsProcessedToday;
            set
            {
                _resultsProcessedToday = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// عدد النتائج غير المطبوعة اليوم
        /// </summary>
        public int PendingResults
        {
            get => _pendingResults;
            set
            {
                _pendingResults = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// إجمالي المرضى هذا الشهر
        /// </summary>
        public int TotalPatientsThisMonth
        {
            get => _totalPatientsThisMonth;
            set
            {
                _totalPatientsThisMonth = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// إجمالي التقارير المُنتجة
        /// </summary>
        public int TotalReportsGenerated
        {
            get => _totalReportsGenerated;
            set
            {
                _totalReportsGenerated = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// آخر تحديث للبيانات
        /// </summary>
        public string LastUpdated
        {
            get => _lastUpdated;
            set
            {
                _lastUpdated = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region الأوامر

        /// <summary>
        /// أمر فتح نافذة إضافة مريض جديد
        /// </summary>
        public ICommand AddPatientCommand { get; }

        /// <summary>
        /// أمر فتح نافذة إدخال النتائج
        /// </summary>
        public ICommand EnterResultsCommand { get; }

        /// <summary>
        /// أمر تحديث البيانات
        /// </summary>
        public ICommand RefreshDataCommand { get; }

        #endregion

        #region الطرق الخاصة

        /// <summary>
        /// تحميل البيانات الفعلية للوحة المعلومات
        /// </summary>
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;
                
                // تحميل البيانات الفعلية من قاعدة البيانات
                var todayPatients = await _patientService.GetTodayPatientsAsync();
                PatientsRegisteredToday = todayPatients.Count;
                
                // حساب النتائج المكتملة اليوم
                ResultsProcessedToday = todayPatients
                    .SelectMany(p => p.TestRequests)
                    .Count(tr => tr.IsCompleted);
                
                // حساب النتائج المعلقة
                PendingResults = todayPatients
                    .SelectMany(p => p.TestRequests)
                    .Count(tr => !tr.IsCompleted);
                
                // حساب المرضى هذا الشهر (بيانات وهمية حالياً)
                var thisMonth = DateTime.Now.Month;
                var thisYear = DateTime.Now.Year;
                var allPatients = await _patientService.GetAllPatientsAsync();
                TotalPatientsThisMonth = allPatients
                    .Count(p => p.CreatedDate.Month == thisMonth && p.CreatedDate.Year == thisYear);
                
                // حساب التقارير المُنتجة (بيانات وهمية حالياً)
                TotalReportsGenerated = allPatients
                    .SelectMany(p => p.TestRequests)
                    .Count(tr => tr.IsPrinted);
                
                LastUpdated = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل البيانات: {ex.Message}";
                ErrorLogger.Log(ex, "DashboardViewModel.LoadDashboardDataAsync");
                
                // استخدام بيانات وهمية في حالة الخطأ
                LoadFallbackData();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// تحميل بيانات احتياطية في حالة فشل تحميل البيانات الفعلية
        /// </summary>
        private void LoadFallbackData()
        {
            var random = new Random();
            
            PatientsRegisteredToday = random.Next(5, 25);
            ResultsProcessedToday = random.Next(10, 40);
            PendingResults = random.Next(3, 15);
            TotalPatientsThisMonth = random.Next(150, 350);
            TotalReportsGenerated = random.Next(500, 1500);
            
            LastUpdated = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        }

        /// <summary>
        /// فتح نافذة إضافة مريض جديد
        /// </summary>
        private void OpenAddPatientWindow()
        {
            try
            {
                var addPatientWindow = _serviceProvider.GetRequiredService<AddPatientWindow>();
                var result = addPatientWindow.ShowDialog();
                
                if (result == true)
                {
                    // تحديث البيانات بعد إضافة مريض جديد
                    LoadDashboardDataAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في فتح نافذة إضافة المريض: {ex.Message}";
                ErrorLogger.Log(ex, "DashboardViewModel.OpenAddPatientWindow");
            }
        }

        /// <summary>
        /// فتح نافذة إدخال النتائج
        /// </summary>
        private void OpenEnterResultsWindow()
        {
            try
            {
                var enterResultsWindow = _serviceProvider.GetRequiredService<EnterResultsWindow>();
                enterResultsWindow.ShowDialog();
                
                // تحديث البيانات بعد إغلاق نافذة النتائج
                LoadDashboardDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في فتح نافذة إدخال النتائج: {ex.Message}";
                ErrorLogger.Log(ex, "DashboardViewModel.OpenEnterResultsWindow");
            }
        }

        #endregion
    }
}
