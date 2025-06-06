using System;
using System.Windows.Controls;

namespace OGRALAB.Services
{
    /// <summary>
    /// واجهة خدمة التنقل بين المحتويات المختلفة في التطبيق
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// حدث يتم إطلاقه عند تغيير المحتوى
        /// </summary>
        event EventHandler<UserControl>? ContentChanged;

        /// <summary>
        /// التنقل إلى محتوى جديد
        /// </summary>
        /// <param name="content">المحتوى المراد عرضه</param>
        void NavigateTo(UserControl content);

        /// <summary>
        /// الحصول على المحتوى الحالي
        /// </summary>
        UserControl? CurrentContent { get; }

        /// <summary>
        /// التنقل إلى لوحة المعلومات
        /// </summary>
        void NavigateToDashboard();

        /// <summary>
        /// *** دالة جديدة: التنقل إلى واجهة محددة باسمها الرمزي ***
        /// </summary>
        /// <param name="viewName">الاسم الرمزي للواجهة (مثل "AddPatient", "EnterResults")</param>
        void NavigateToView(string viewName); // ***** السطر الجديد المضاف *****

        /// <summary>
        /// تسجيل الخروج من التطبيق
        /// </summary>
        void Logout();
    }
}