using System.Windows.Controls;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    /// <summary>
    /// عنصر تحكم لوحة المعلومات الرئيسية
    /// </summary>
    public partial class DashboardUserControl : UserControl
    {
        public DashboardUserControl(DashboardViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
