using System;
using System.Windows;
using System.Windows.Controls;
using OGRALAB.Views;

namespace OGRALAB.Services
{
    /// <summary>
    /// خدمة التنقل بين المحتويات المختلفة في التطبيق
    /// </summary>
    public class NavigationService : INavigationService
    {
        private UserControl? _currentContent;

        /// <summary>
        /// حدث يتم إطلاقه عند تغيير المحتوى
        /// </summary>
        public event EventHandler<UserControl>? ContentChanged;

        /// <summary>
        /// الحصول على المحتوى الحالي
        /// </summary>
        public UserControl? CurrentContent 
        { 
            get => _currentContent;
            private set
            {
                _currentContent = value;
                if (value != null)
                    ContentChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// التنقل إلى محتوى جديد
        /// </summary>
        /// <param name="content">المحتوى المراد عرضه</param>
        public void NavigateTo(UserControl content)
        {
            if (content != null)
            {
                CurrentContent = content;
            }
        }

        /// <summary>
        /// التنقل إلى لوحة المعلومات
        /// </summary>
        public void NavigateToDashboard()
        {
            var dashboardControl = new DashboardUserControl();
            NavigateTo(dashboardControl);
        }

        /// <summary>
        /// تسجيل الخروج من التطبيق
        /// </summary>
        public void Logout()
        {
            try
            {
                // إغلاق التطبيق
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "NavigationService.Logout");
                throw;
            }
        }
    }
}
