using System;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using OGRALAB.Services;
using OGRALAB.Views;
namespace OGRALAB.ViewModels
{
    /// <summary>
    /// نموذج عرض واجهة الإعدادات الرئيسية
    /// يتعامل مع فتح النوافذ الفرعية لإدارة المستخدمين والتحاليل
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthenticationService _authenticationService;
        public SettingsViewModel(IServiceProvider serviceProvider, IAuthenticationService authenticationService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            InitializeCommands();
        }
        #region Commands
        /// <summary>
        /// أمر فتح إدارة المستخدمين
        /// </summary>
        public ICommand OpenUserManagementCommand { get; private set; } = null!;
        /// <summary>
        /// أمر فتح إدارة أنواع التحاليل
        /// </summary>
        public ICommand OpenTestManagementCommand { get; private set; } = null!;
        #endregion
        #region Properties
        /// <summary>
        /// هل المستخدم الحالي مسؤول؟
        /// يحدد إمكانية الوصول لإدارة المستخدمين
        /// </summary>
        public bool IsCurrentUserAdmin => _authenticationService.CurrentUser?.IsAdmin ?? false;
        /// <summary>
        /// اسم المستخدم الحالي
        /// </summary>
        public string CurrentUserName => _authenticationService.CurrentUser?.DisplayName ?? "مستخدم";
        /// <summary>
        /// نوع صلاحيات المستخدم الحالي
        /// </summary>
        public string CurrentUserRole => _authenticationService.CurrentUser?.RoleDescription ?? "";
        #endregion
        #region Methods
        /// <summary>
        /// تهيئة الأوامر
        /// </summary>
        private void InitializeCommands()
        {
            OpenUserManagementCommand = new RelayCommand(OpenUserManagement, CanAccessUserManagement);
            OpenTestManagementCommand = new RelayCommand(OpenTestManagement);
        }
        /// <summary>
        /// فحص إمكانية الوصول لإدارة المستخدمين
        /// فقط المسؤولون يمكنهم الوصول
        /// </summary>
        /// <returns>true إذا كان المستخدم مسؤولاً</returns>
        private bool CanAccessUserManagement()
        {
            return IsCurrentUserAdmin;
        }
        /// <summary>
        /// فتح نافذة إدارة المستخدمين
        /// </summary>
        private void OpenUserManagement()
        {
            try
            {
                if (!CanAccessUserManagement())
                {
                    System.Windows.MessageBox.Show("ليس لديك صلاحية للوصول إلى إدارة المستخدمين.\nهذه الوظيفة متاحة فقط للمسؤولين.",
                                                  "عدم وجود صلاحية",
                                                  System.Windows.MessageBoxButton.OK,
                                                  System.Windows.MessageBoxImage.Warning);
                    return;
                }
                // إنشاء النافذة والـ ViewModel
                var window = _serviceProvider.GetRequiredService<UserManagementWindow>();
                var viewModel = _serviceProvider.GetRequiredService<UserManagementViewModel>();

                // ربط الـ ViewModel بالنافذة
                window.DataContext = viewModel;

                // عرض النافذة كـ Dialog
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "SettingsViewModel.OpenUserManagement");
                System.Windows.MessageBox.Show($"حدث خطأ أثناء فتح إدارة المستخدمين:\n{ex.Message}",
                                              "خطأ",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// فتح نافذة إدارة أنواع التحاليل
        /// </summary>
        private void OpenTestManagement()
        {
            try
            {
                // إنشاء النافذة والـ ViewModel
                var window = _serviceProvider.GetRequiredService<TestManagementWindow>();
                var viewModel = _serviceProvider.GetRequiredService<TestManagementViewModel>();

                // ربط الـ ViewModel بالنافذة
                window.DataContext = viewModel;

                // عرض النافذة كـ Dialog
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "SettingsViewModel.OpenTestManagement");
                System.Windows.MessageBox.Show($"حدث خطأ أثناء فتح إدارة التحاليل:\n{ex.Message}",
                                              "خطأ",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }
        #endregion
        #region INotifyPropertyChanged
        /// <summary>
        /// تحديث خصائص المستخدم عند تغيير المستخدم الحالي
        /// </summary>
        public void RefreshUserInfo()
        {
            OnPropertyChanged(nameof(IsCurrentUserAdmin));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUserRole));

            // تحديث حالة الأوامر
            if (OpenUserManagementCommand is RelayCommand userMgmtCmd)
                userMgmtCmd.RaiseCanExecuteChanged();
        }
        #endregion
    }
    /// <summary>
    /// أمر بسيط لتنفيذ الإجراءات
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }
        public void Execute(object? parameter)
        {
            _execute();
        }
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}