using System.Windows;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    /// <summary>
    /// نافذة إدخال النتائج
    /// </summary>
    public partial class EnterResultsWindow : Window
    {
        public EnterResultsWindow(EnterResultsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // تحميل البيانات عند فتح النافذة
            Loaded += async (s, e) => 
            {
                if (viewModel.LoadDataCommand is Commands.AsyncRelayCommand asyncCommand)
                {
                    await asyncCommand.ExecuteAsync(null);
                }
            };
        }
    }
}
