using System.Collections.ObjectModel;
using System.Windows.Controls;
using OGRALAB.Commands;
using OGRALAB.Models;
using OGRALAB.Services;
using System;
using System.Linq;

namespace OGRALAB.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private User? _currentUser;
        private readonly INavigationService _navigationService;
        private string _welcomeMessage = string.Empty;
        private UserControl? _currentContent;
        private NavigationItem? _selectedMenuItem;
        private bool _showDashboardStats;

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _navigationService.ContentChanged += OnContentChanged;
        }

        public void InitializeForUser(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            WelcomeMessage = $"مرحباً، {_currentUser.FullName}!";
            InitializeCommands();
            InitializeMenuItems();
            NavigateToDashboard();
        }

        private void OnContentChanged(object? sender, UserControl? content)
        {
            CurrentContent = content;
        }

        #region خصائص
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }
        public User? CurrentUser => _currentUser;
        public UserControl? CurrentContent
        {
            get => _currentContent;
            set => SetProperty(ref _currentContent, value);
        }
        public NavigationItem? SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (SetProperty(ref _selectedMenuItem, value) && value != null && value.IsEnabled)
                {
                    // هذا الجزء من الكود الأصلي أفضل لأنه ينفذ الأمر عند التحديد
                    value.Command?.Execute(null);

                    // هذا الجزء من كود الوكيل لضمان التحديد البصري فقط
                    foreach (var item in MenuItems.Where(i => i != value))
                    {
                        item.IsSelected = false;
                    }
                    value.IsSelected = true;
                }
            }
        }
        public bool ShowDashboardStats
        {
            get => _showDashboardStats;
            set => SetProperty(ref _showDashboardStats, value);
        }
        public ObservableCollection<NavigationItem> MenuItems { get; } = new ObservableCollection<NavigationItem>();
        #endregion

        #region الأوامر
        public RelayCommand? DashboardCommand { get; private set; }
        public RelayCommand? AddPatientsCommand { get; private set; }
        public RelayCommand? EnterResultsCommand { get; private set; }
        public RelayCommand? PreviewReportsCommand { get; private set; }
        public RelayCommand? SearchModifyCommand { get; private set; }
        public RelayCommand? SettingsCommand { get; private set; }
        public RelayCommand? LogoutCommand { get; private set; }

        private void InitializeCommands()
        {
            DashboardCommand = new RelayCommand(NavigateToDashboard, () => _currentUser != null);
            AddPatientsCommand = new RelayCommand(NavigateToAddPatients, () => _currentUser != null);
            EnterResultsCommand = new RelayCommand(NavigateToEnterResults, () => _currentUser != null);
            PreviewReportsCommand = new RelayCommand(NavigateToPreviewReports, () => false); // معطل حاليًا
            SearchModifyCommand = new RelayCommand(NavigateToSearchModify, () => false); // معطل حاليًا

            // *** تم تفعيل أمر الإعدادات ***
            SettingsCommand = new RelayCommand(NavigateToSettings, () => _currentUser != null);

            LogoutCommand = new RelayCommand(Logout, () => _currentUser != null);
        }

        private void InitializeMenuItems()
        {
            MenuItems.Clear();
            if (_currentUser == null) return;

            MenuItems.Add(new NavigationItem { Icon = "📊", Title = "لوحة المعلومات", Command = DashboardCommand, IsEnabled = true, IsSelected = true });
            MenuItems.Add(new NavigationItem { Icon = "👥", Title = "إدخال المرضى", Command = AddPatientsCommand, IsEnabled = true });
            MenuItems.Add(new NavigationItem { Icon = "📝", Title = "إدخال النتائج", Command = EnterResultsCommand, IsEnabled = true });
            MenuItems.Add(new NavigationItem { Icon = "📄", Title = "معاينة وطباعة التقارير", Command = PreviewReportsCommand, IsEnabled = false });
            MenuItems.Add(new NavigationItem { Icon = "🔍", Title = "البحث والتعديل", Command = SearchModifyCommand, IsEnabled = false });

            // *** تم تفعيل عنصر الإعدادات ***
            MenuItems.Add(new NavigationItem { Icon = "⚙️", Title = "الإعدادات", Command = SettingsCommand, IsEnabled = true });

            MenuItems.Add(new NavigationItem { Icon = "🚪", Title = "تسجيل الخروج", Command = LogoutCommand, IsEnabled = true });

            SelectedMenuItem = MenuItems.FirstOrDefault(m => m.IsSelected);
        }
        #endregion

        #region طرق التنقل
        private void NavigateToDashboard()
        {
            if (_currentUser != null)
            {
                _navigationService.NavigateToDashboard();
                ShowDashboardStats = true;
            }
        }
        private void NavigateToAddPatients()
        {
            if (_currentUser != null)
            {
                _navigationService.NavigateToView("AddPatient");
                ShowDashboardStats = false;
            }
        }
        private void NavigateToEnterResults()
        {
            if (_currentUser != null)
            {
                _navigationService.NavigateToView("EnterResults");
                ShowDashboardStats = false;
            }
        }
        private void NavigateToPreviewReports()
        {
            ShowDashboardStats = false;
        }
        private void NavigateToSearchModify()
        {
            ShowDashboardStats = false;
        }

        // *** تم تطوير طريقة التنقل للإعدادات ***
        private void NavigateToSettings()
        {
            if (_currentUser != null)
            {
                _navigationService.NavigateToView("Settings");
                ShowDashboardStats = false;
            }
        }

        private void Logout()
        {
            if (_currentUser != null)
            {
                _navigationService.Logout();
                ShowDashboardStats = false;
            }
        }
        #endregion
    }

    public class NavigationItem : BaseViewModel
    {
        private bool _isSelected;
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public RelayCommand? Command { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}