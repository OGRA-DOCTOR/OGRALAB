using System;
using System.Windows;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    /// <summary>
    /// نافذة إضافة مريض جديد
    /// </summary>
    public partial class AddPatientWindow : Window
    {
        public AddPatientWindow(AddPatientViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // الاشتراك في أحداث ViewModel
            viewModel.PatientSaved += OnPatientSaved;
            viewModel.CancelRequested += OnCancelRequested;

            // تحميل البيانات عند فتح النافذة
            Loaded += async (s, e) => 
            {
                if (viewModel.LoadDataCommand is Commands.AsyncRelayCommand asyncCommand)
                {
                    await asyncCommand.ExecuteAsync(null);
                }
            };
        }

        /// <summary>
        /// معالج حدث حفظ المريض بنجاح
        /// </summary>
        private void OnPatientSaved(object? sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// معالج حدث طلب الإلغاء
        /// </summary>
        private void OnCancelRequested(object? sender, EventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // إلغاء الاشتراك في الأحداث لتجنب تسريب الذاكرة
            if (DataContext is AddPatientViewModel viewModel)
            {
                viewModel.PatientSaved -= OnPatientSaved;
                viewModel.CancelRequested -= OnCancelRequested;
            }

            base.OnClosed(e);
        }
    }
}
