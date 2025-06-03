using System.Windows.Controls;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    /// <summary>
    /// عنصر تحكم لوحة المعلومات الرئيسية
    /// </summary>
    public partial class DashboardUserControl : UserControl
    {
        public DashboardUserControl()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }
    }
}
