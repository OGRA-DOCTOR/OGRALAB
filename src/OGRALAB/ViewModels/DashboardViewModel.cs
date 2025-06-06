using System;
using System.ComponentModel;
using OGRALAB.Services;
using OGRALAB.Commands; // *** إضافة هذا السطر لاستخدام RelayCommand ***

namespace OGRALAB.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private int _patientsRegisteredToday;
        private int _resultsProcessedToday;
        private int _pendingResults;
        private int _totalPatientsThisMonth;
        private int _totalReportsGenerated;
        private string _lastUpdated = string.Empty;

        private readonly IServiceProvider? _serviceProvider;
        private readonly PatientService? _patientService;

        // *** الأمر الجديد ***
        public RelayCommand RefreshDataCommand { get; private set; }

        public DashboardViewModel()
        {
            // تهيئة الأمر
            RefreshDataCommand = new RelayCommand(RefreshData);
            LoadDashboardData();
        }

        public DashboardViewModel(IServiceProvider serviceProvider, PatientService patientService)
        {
            _serviceProvider = serviceProvider;
            _patientService = patientService;

            // تهيئة الأمر
            RefreshDataCommand = new RelayCommand(RefreshData);
            LoadDashboardData();
        }


        #region خصائص الإحصائيات
        public int PatientsRegisteredToday
        {
            get => _patientsRegisteredToday;
            set => SetProperty(ref _patientsRegisteredToday, value); // استخدام SetProperty بدلاً من OnPropertyChanged مباشرة
        }

        public int ResultsProcessedToday
        {
            get => _resultsProcessedToday;
            set => SetProperty(ref _resultsProcessedToday, value);
        }

        public int PendingResults
        {
            get => _pendingResults;
            set => SetProperty(ref _pendingResults, value);
        }

        public int TotalPatientsThisMonth
        {
            get => _totalPatientsThisMonth;
            set => SetProperty(ref _totalPatientsThisMonth, value);
        }

        public int TotalReportsGenerated
        {
            get => _totalReportsGenerated;
            set => SetProperty(ref _totalReportsGenerated, value);
        }

        public string LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }
        #endregion

        #region الطرق الخاصة
        private void LoadDashboardData()
        {
            try
            {
                var random = new Random();
                PatientsRegisteredToday = random.Next(5, 25);
                ResultsProcessedToday = random.Next(10, 40);
                PendingResults = random.Next(3, 15);
                TotalPatientsThisMonth = random.Next(150, 350);
                TotalReportsGenerated = random.Next(500, 1500);
                LastUpdated = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "DashboardViewModel.LoadDashboardData");
            }
        }

        public void RefreshData() // هذه الدالة سيتم استدعاؤها بواسطة الأمر
        {
            LoadDashboardData();
            // يمكنك هنا إضافة أي منطق آخر عند تحديث البيانات، مثل إظهار رسالة "تم التحديث" مؤقتًا إذا أردت
        }
        #endregion
    }
}