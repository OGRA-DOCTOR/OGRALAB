using OGRALAB.Commands;
using OGRALAB.Models;
using OGRALAB.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
namespace OGRALAB.ViewModels
{
    public class ReportPreviewViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IPrintService _printService;
        public ReportPreviewViewModel(IReportService reportService, IPrintService printService)
        {
            _reportService = reportService;
            _printService = printService;
            PrintCommand = new AsyncRelayCommand(PrintReportAsync, CanPrint);
            CloseCommand = new RelayCommand(CloseWindow);
        }
        #region Properties
        private PreparedReportData? _reportData;
        public PreparedReportData? ReportData
        {
            get => _reportData;
            set => SetProperty(ref _reportData, value);
        }
        private FlowDocument? _reportDocument;
        public FlowDocument? ReportDocument
        {
            get => _reportDocument;
            set => SetProperty(ref _reportDocument, value);
        }
        private bool _isPrinting;
        public bool IsPrinting
        {
            get => _isPrinting;
            set
            {
                SetProperty(ref _isPrinting, value);
                PrintCommand.RaiseCanExecuteChanged();
            }
        }
        private string _windowTitle = "معاينة التقرير";
        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }
        private string _reportSummary = string.Empty;
        public string ReportSummary
        {
            get => _reportSummary;
            set => SetProperty(ref _reportSummary, value);
        }
        #endregion
        #region Commands
        public AsyncRelayCommand PrintCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        #endregion
        #region Methods
        public void SetReportData(PreparedReportData reportData)
        {
            ReportData = reportData;

            // تحديث عنوان النافذة
            WindowTitle = $"معاينة التقرير - {reportData.Patient.FullName} ({reportData.Patient.PatientCode})";

            // تحديث ملخص التقرير
            var testsCount = reportData.SelectedTests.Count;
            var abnormalCount = reportData.SelectedTests.Count(t => t.IsAbnormal);
            ReportSummary = $"عدد الفحوصات: {testsCount} | النتائج غير الطبيعية: {abnormalCount}";

            // إنشاء مستند التقرير
            GenerateReportDocument();
        }
        private void GenerateReportDocument()
        {
            if (ReportData == null)
                return;
            try
            {
                ReportDocument = _reportService.CreateReportDocument(ReportData);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ReportPreviewViewModel.GenerateReportDocument");
                MessageBox.Show($"حدث خطأ أثناء إنشاء مستند التقرير: {ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanPrint()
        {
            return ReportDocument != null &&
                   !IsPrinting &&
                   _printService.IsDefaultPrinterAvailable();
        }
        private async Task PrintReportAsync()
        {
            if (ReportDocument == null)
                return;
            try
            {
                IsPrinting = true;

                var success = await _printService.PrintDocumentAsync(ReportDocument);

                if (success)
                {
                    MessageBox.Show("تم إرسال التقرير للطباعة بنجاح.", "نجح",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("تم إلغاء عملية الطباعة.", "ملغاة",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ReportPreviewViewModel.PrintReportAsync");
                MessageBox.Show($"حدث خطأ أثناء الطباعة: {ex.Message}",
                               "خطأ في الطباعة", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsPrinting = false;
            }
        }
        private void CloseWindow()
        {
            // إغلاق النافذة
            var window = Application.Current.Windows.OfType<Views.ReportPreviewWindow>().FirstOrDefault();
            window?.Close();
        }
        #endregion
    }
}