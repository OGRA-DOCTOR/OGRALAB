using System;
using System.Windows;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    /// <summary>
    /// النافذة الرئيسية للتطبيق مع قائمة التنقل الجانبية
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private INavigationService? _navigationService;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(User currentUser) : this()
        {
            InitializeWithUser(currentUser);
        }

        public MainWindow(MainViewModel viewModel) : this()
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        /// <summary>
        /// تهيئة النافذة مع المستخدم الحالي
        /// </summary>
        /// <param name="currentUser">المستخدم الحالي</param>
        private void InitializeWithUser(User currentUser)
        {
            try
            {
                // إنشاء خدمة التنقل
                _navigationService = new NavigationService();
                
                // إنشاء نموذج العرض الرئيسي
                _viewModel = new MainViewModel(currentUser, _navigationService);
                DataContext = _viewModel;
                
                // تسجيل معالج إغلاق النافذة
                Closed += MainWindow_Closed;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "MainWindow.InitializeWithUser");
                MessageBox.Show($"خطأ في تهيئة النافذة الرئيسية: {ex.Message}", 
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// معالج إغلاق النافذة
        /// </summary>
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            try
            {
                // تنظيف الموارد
                _viewModel = null;
                _navigationService = null;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "MainWindow.MainWindow_Closed");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // تنظيف إضافي إذا لزم الأمر
            base.OnClosed(e);
        }
    }
}
