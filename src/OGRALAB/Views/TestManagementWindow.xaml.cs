using OGRALAB.ViewModels;
using System;
using System.Windows;
namespace OGRALAB.Views
{
    /// <summary>
    /// نافذة إدارة التحاليل والمعدلات الطبيعية
    /// تدير عمليات إضافة وتعديل وحذف التحاليل ومعدلاتها الطبيعية المرنة
    /// </summary>
    public partial class TestManagementWindow : Window
    {
        public TestManagementWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// معالج تحميل النافذة
        /// تهيئة العناصر والبيانات الأولية
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // تحميل البيانات عند فتح النافذة
            if (DataContext is TestManagementViewModel viewModel)
            {
                // يتم تحميل البيانات تلقائياً في المنشئ (constructor)
                // لا نحتاج إجراءات إضافية هنا
            }
        }
        /// <summary>
        /// معالج إغلاق النافذة
        /// تنظيف الموارد قبل الإغلاق
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // تنظيف الموارد إذا لزم الأمر
            base.OnClosed(e);
        }
    }
}