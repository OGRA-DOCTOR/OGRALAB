using System;
using System.ComponentModel;

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

        public DashboardViewModel()
        {
            LoadDashboardData();
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

        #region الطرق الخاصة

        /// <summary>
        /// تحميل البيانات الوهمية للوحة المعلومات
        /// </summary>
        private void LoadDashboardData()
        {
            try
            {
                // إنشاء بيانات وهمية للعرض
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

        /// <summary>
        /// تحديث البيانات
        /// </summary>
        public void RefreshData()
        {
            LoadDashboardData();
        }

        #endregion
    }
}
