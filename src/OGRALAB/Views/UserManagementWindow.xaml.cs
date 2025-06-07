using OGRALAB.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
namespace OGRALAB.Views
{
    /// <summary>
    /// نافذة إدارة المستخدمين
    /// تدير عمليات إضافة وتعديل وحذف المستخدمين والصلاحيات
    /// </summary>
    public partial class UserManagementWindow : Window
    {
        public UserManagementWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// معالج تغيير كلمة المرور
        /// ربط PasswordBox مع ViewModel
        /// </summary>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }
        /// <summary>
        /// معالج تغيير تأكيد كلمة المرور
        /// ربط PasswordBox مع ViewModel
        /// </summary>
        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.ConfirmPassword = passwordBox.Password;
            }
        }
        /// <summary>
        /// معالج إغلاق النافذة
        /// تنظيف الموارد قبل الإغلاق
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // تنظيف كلمات المرور من الذاكرة
            PasswordBox.Clear();
            ConfirmPasswordBox.Clear();

            base.OnClosed(e);
        }
        /// <summary>
        /// معالج تحميل النافذة
        /// التركيز على أول عنصر إدخال
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // لا نحتاج تركيز خاص لأن البيانات تُحمل تلقائياً
        }
    }
}