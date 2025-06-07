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
                        var dashboardControl = _serviceProvider.GetRequiredService<DashboardUserControl>();
                        dashboardControl.DataContext = dashboardViewModel;
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

                    // --- تم تعليق هذا الجزء مؤقتًا لأنه لم يتم إنشاؤه بعد ---
                    /*
                    case "Settings":
                        var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
                        var settingsControl = _serviceProvider.GetRequiredService<SettingsUserControl>();
                        settingsControl.DataContext = settingsViewModel;
                        viewToNavigate = settingsControl;
                        break;
                    */

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
                // افترض وجود ErrorLogger لديك
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