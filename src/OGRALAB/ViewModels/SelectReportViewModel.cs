using OGRALAB.Commands;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OGRALAB.ViewModels
{
    public class SelectReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IServiceProvider _serviceProvider;

        public SelectReportViewModel(IReportService reportService, IServiceProvider serviceProvider)
        {
            _reportService = reportService;
            _serviceProvider = serviceProvider;
            Patients = new ObservableCollection<PatientReportData>();

            PreviewReportCommand = new AsyncRelayCommand(PreviewReportAsync, CanPreviewReport);
            RefreshCommand = new AsyncRelayCommand(LoadPatientsAsync);

            // تم التعديل هنا: لا يمكن استخدام await في المنشئ مباشرة
            // سيتم تشغيل المهمة في الخلفية وستقوم الواجهة بتحديث نفسها عند اكتمالها
            _ = LoadPatientsAsync();
        }

        #region Properties
        public ObservableCollection<PatientReportData> Patients { get; set; }

        private PatientReportData? _selectedPatient;
        public PatientReportData? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                SetProperty(ref _selectedPatient, value);
                if (value != null)
                {
                    // تم التعديل هنا: تشغيل المهمة في الخلفية لتجنب تجميد الواجهة
                    _ = LoadPatientTestsAsync(value.PatientId);
                }
                UpdateCommandStates();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                UpdateCommandStates();
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private int _selectedTestsCount;
        public int SelectedTestsCount
        {
            get => _selectedTestsCount;
            set
            {
                SetProperty(ref _selectedTestsCount, value);
                UpdateCommandStates();
            }
        }
        #endregion

        #region Commands
        public AsyncRelayCommand PreviewReportCommand { get; private set; }
        public AsyncRelayCommand RefreshCommand { get; private set; }
        #endregion

        #region Methods
        private async Task LoadPatientsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل قائمة المرضى...";
                Patients.Clear();

                var patients = await _reportService.GetPatientsForReportsAsync();

                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                }

                if (patients.Any())
                {
                    StatusMessage = $"تم تحميل {patients.Count} مريض";
                }
                else
                {
                    StatusMessage = "لا يوجد مرضى بفحوصات مسجلة";
                }

                await Task.Delay(3000);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "SelectReportViewModel.LoadPatientsAsync");
                StatusMessage = "حدث خطأ أثناء تحميل قائمة المرضى";
                MessageBox.Show($"حدث خطأ أثناء تحميل قائمة المرضى: {ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPatientTestsAsync(int patientId)
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل فحوصات المريض...";

                var patientData = await _reportService.GetPatientReportDataAsync(patientId);

                var currentSelectedPatient = Patients.FirstOrDefault(p => p.PatientId == patientId);
                if (currentSelectedPatient != null)
                {
                    // إلغاء الاشتراك من الأحداث القديمة لمنع تسرب الذاكرة
                    foreach (var oldTest in currentSelectedPatient.AvailableTests)
                    {
                        oldTest.PropertyChanged -= OnTestSelectionChanged;
                    }

                    currentSelectedPatient.AvailableTests.Clear();
                    foreach (var test in patientData.AvailableTests)
                    {
                        test.PropertyChanged += OnTestSelectionChanged;
                        currentSelectedPatient.AvailableTests.Add(test);
                    }

                    // تحديث عدد الاختبارات المختارة بعد تحميلها
                    UpdateSelectedTestsCount();

                    var completedTestsCount = patientData.AvailableTests.Count(t => t.HasResult);
                    var totalTestsCount = patientData.AvailableTests.Count;
                    StatusMessage = $"تم تحميل {totalTestsCount} فحص، {completedTestsCount} منها مكتمل";
                }

                await Task.Delay(3000);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"SelectReportViewModel.LoadPatientTestsAsync({patientId})");
                StatusMessage = "حدث خطأ أثناء تحميل فحوصات المريض";
                MessageBox.Show($"حدث خطأ أثناء تحميل فحوصات المريض: {ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnTestSelectionChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TestReportItem.IsSelected))
            {
                UpdateSelectedTestsCount();
            }
        }

        private void UpdateSelectedTestsCount()
        {
            if (SelectedPatient != null)
            {
                SelectedTestsCount = SelectedPatient.AvailableTests.Count(t => t.IsSelected && t.HasResult);
            }
            else
            {
                SelectedTestsCount = 0;
            }
        }

        private void UpdateCommandStates()
        {
            PreviewReportCommand.RaiseCanExecuteChanged();
            RefreshCommand.RaiseCanExecuteChanged();
        }

        private bool CanPreviewReport()
        {
            return !IsLoading &&
                   SelectedPatient != null &&
                   SelectedTestsCount > 0;
        }

        private async Task PreviewReportAsync()
        {
            if (SelectedPatient == null) return;

            try
            {
                var selectedTests = SelectedPatient.AvailableTests.Where(t => t.IsSelected && t.HasResult).ToList();

                if (!selectedTests.Any())
                {
                    MessageBox.Show("يجب اختيار فحص واحد على الأقل من الفحوصات المكتملة.",
                                   "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                StatusMessage = "جاري إعداد التقرير...";
                var reportData = _reportService.PrepareReportData(SelectedPatient, selectedTests);

                var previewWindow = _serviceProvider.GetRequiredService<ReportPreviewWindow>();
                var previewViewModel = (ReportPreviewViewModel)previewWindow.DataContext;

                previewViewModel.SetReportData(reportData);

                previewWindow.Show();

                StatusMessage = "تم فتح نافذة معاينة التقرير";
                await Task.Delay(3000);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "SelectReportViewModel.PreviewReportAsync");
                MessageBox.Show($"حدث خطأ أثناء إنشاء التقرير: {ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "فشل إنشاء التقرير.";
            }
        }
        #endregion
    }
}