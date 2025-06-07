using System.Windows;
namespace OGRALAB.Views
{
    /// <summary>
    /// نافذة معاينة التقرير
    /// </summary>
    public partial class ReportPreviewWindow : Window
    {
        public ReportPreviewWindow()
        {
            InitializeComponent();
        }
        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);

            // تحسين عرض النافذة
            this.WindowState = WindowState.Maximized;
        }
    }
}