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
                    // *** تعديل هنا لمعالجة التحذير ***
                    if (_currentContent != null)
                    {
                        ContentChanged?.Invoke(this, _currentContent);
                    }
                    else
                    {
                        // يمكنك اختيار إطلاق الحدث مع null إذا كان هذا هو السلوك المطلوب
                        // أو ببساطة عدم إطلاقه إذا كان المحتوى null
                        // ContentChanged?.Invoke(this, null); // إذا أردت إعلام المشتركين بأن المحتوى أصبح null
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
                ErrorLogger.Log(ex, $"NavigationService.NavigateToView({viewName})");
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
                ErrorLogger.Log(ex, "NavigationService.Logout");
                MessageBox.Show($"Error during logout: {ex.Message}", "Logout Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}