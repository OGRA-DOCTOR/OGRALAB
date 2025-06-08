using Microsoft.Extensions.DependencyInjection;
using OGRALAB.ViewModels;
using OGRALAB.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OGRALAB.Services
{
    public class NavigationService : INavigationService
    {
        private UserControl? _currentContent;
        private readonly IServiceProvider _serviceProvider;
        public event EventHandler<UserControl>? ContentChanged;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public UserControl? CurrentContent
        {
            get => _currentContent;
            private set
            {
                if (_currentContent != value)
                {
                    _currentContent = value;
                    if (_currentContent != null)
                    {
                        ContentChanged?.Invoke(this, _currentContent);
                    }
                }
            }
        }

        public void NavigateTo(UserControl content)
        {
            CurrentContent = content;
        }

        public void NavigateToDashboard()
        {
            NavigateToView("Dashboard");
        }

        public void NavigateToView(string viewName)
        {
            try
            {
                UserControl? viewToNavigate = null;
                switch (viewName)
                {
                    case "Dashboard":
                        var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
                        var dashboardControl = new DashboardUserControl(dashboardViewModel);
                        viewToNavigate = dashboardControl;
                        break;
                    case "AddPatient":
                        var addPatientViewModel = _serviceProvider.GetRequiredService<AddPatientViewModel>();
                        var addPatientControl = _serviceProvider.GetRequiredService<AddPatientUserControl>();
                        addPatientControl.DataContext = addPatientViewModel;
                        viewToNavigate = addPatientControl;
                        break;
                    case "EnterResults":
                        var enterResultsViewModel = _serviceProvider.GetRequiredService<EnterResultsViewModel>();
                        var enterResultsControl = _serviceProvider.GetRequiredService<EnterResultsUserControl>();
                        enterResultsControl.DataContext = enterResultsViewModel;
                        viewToNavigate = enterResultsControl;
                        break;
                    // *** تم إلغاء تعليق واجهة الإعدادات ***
                    case "Settings":
                        var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
                        var settingsControl = _serviceProvider.GetRequiredService<SettingsUserControl>();
                        settingsControl.DataContext = settingsViewModel;
                        viewToNavigate = settingsControl;
                        break;
                    // *** إضافة واجهة القائمة الرئيسية ***
                    case "MainMenu":
                        var mainMenuControl = _serviceProvider.GetRequiredService<MainMenuUserControl>();
                        viewToNavigate = mainMenuControl;
                        break;
                    // *** إضافة واجهة إدارة المستخدمين المضمنة ***
                    case "UserManagementEmbedded":
                        var userMgmtViewModel = _serviceProvider.GetRequiredService<UserManagementViewModel>();
                        var userMgmtControl = _serviceProvider.GetRequiredService<UserManagementUserControl>();
                        userMgmtControl.DataContext = userMgmtViewModel;
                        viewToNavigate = userMgmtControl;
                        break;
                    // *** إضافة واجهة إدارة التحاليل المضمنة ***
                    case "TestManagementEmbedded":
                        var testMgmtViewModel = _serviceProvider.GetRequiredService<TestManagementViewModel>();
                        var testMgmtControl = _serviceProvider.GetRequiredService<TestManagementUserControl>();
                        testMgmtControl.DataContext = testMgmtViewModel;
                        viewToNavigate = testMgmtControl;
                        break;
                    default:
                        MessageBox.Show($"View '{viewName}' not found.", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
                if (viewToNavigate != null)
                {
                    NavigateTo(viewToNavigate);
                }
            }
            catch (Exception ex)
            {
                // ErrorLogger.Log(ex, $"NavigationService.NavigateToView({viewName})");
                MessageBox.Show($"Error navigating to view '{viewName}': {ex.Message}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Logout()
        {
            try
            {
                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mainWindow?.Close();
                Application.Current.MainWindow = loginWindow;
                loginWindow.Show();
                CurrentContent = null;
            }
            catch (Exception ex)
            {
                // ErrorLogger.Log(ex, "NavigationService.Logout");
                MessageBox.Show($"Error during logout: {ex.Message}", "Logout Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}