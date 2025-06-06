using System.Collections.ObjectModel;
using System.Windows.Controls;
using OGRALAB.Commands;
using OGRALAB.Models;
using OGRALAB.Services;
using System;
using System.Linq;
// using OGRALAB.Views; // قد لا يكون هذا مطلوبًا هنا إذا كان NavigationService يعتني بإنشاء Views

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
                    value.Command?.Execute(null);
                }
            }
        }

        public ObservableCollection<NavigationItem> MenuItems { get; private set; } = new ObservableCollection<NavigationItem>();

        public bool ShowDashboardStats
        {
            get => _showDashboardStats;
            set => SetProperty(ref _showDashboardStats, value);
        }
        #endregion

        #region الأوامر
        public RelayCommand DashboardCommand { get; private set; } = null!;
        public RelayCommand AddPatientsCommand { get; private set; } = null!;
        public RelayCommand EnterResultsCommand { get; private set; } = null!;
        public RelayCommand PreviewReportsCommand { get; private set; } = null!;
        public RelayCommand SearchModifyCommand { get; private set; } = null!;
        public RelayCommand SettingsCommand { get; private set; } = null!;
        public RelayCommand LogoutCommand { get; private set; } = null!;
        #endregion

        #region طرق التهيئة
        private void InitializeCommands()
        {
            DashboardCommand = new RelayCommand(NavigateToDashboard, () => _currentUser != null);
            // *** تعديل: تفعيل الأوامر ***
            AddPatientsCommand = new RelayCommand(NavigateToAddPatients, () => _currentUser != null);
            EnterResultsCommand = new RelayCommand(NavigateToEnterResults, () => _currentUser != null);

            // الأوامر الأخرى يمكن تفعيلها بنفس الطريقة عند الحاجة
            PreviewReportsCommand = new RelayCommand(NavigateToPreviewReports, () => false);
            SearchModifyCommand = new RelayCommand(NavigateToSearchModify, () => false);
            SettingsCommand = new RelayCommand(NavigateToSettings, () => false);
            LogoutCommand = new RelayCommand(Logout, () => _currentUser != null);
        }

        private void InitializeMenuItems()
        {
            MenuItems.Clear();
            if (_currentUser == null) return;

            MenuItems.Add(new NavigationItem { Icon = "📊", Title = "لوحة المعلومات", Command = DashboardCommand, IsEnabled = true, IsSelected = true });
            // *** تعديل: تفعيل العناصر في القائمة ***
            MenuItems.Add(new NavigationItem { Icon = "👥", Title = "إدخال المرضى", Command = AddPatientsCommand, IsEnabled = true });
            MenuItems.Add(new NavigationItem { Icon = "📝", Title = "إدخال النتائج", Command = EnterResultsCommand, IsEnabled = true });

            MenuItems.Add(new NavigationItem { Icon = "📄", Title = "معاينة وطباعة التقارير", Command = PreviewReportsCommand, IsEnabled = false });
            MenuItems.Add(new NavigationItem { Icon = "🔍", Title = "البحث والتعديل", Command = SearchModifyCommand, IsEnabled = false });
            MenuItems.Add(new NavigationItem { Icon = "⚙️", Title = "الإعدادات", Command = SettingsCommand, IsEnabled = false });
            MenuItems.Add(new NavigationItem { Icon = "🚪", Title = "تسجيل الخروج", Command = LogoutCommand, IsEnabled = true });

            SelectedMenuItem = MenuItems.FirstOrDefault(m => m.IsSelected);
        }
        #endregion

        #region طرق التنقل
        private void NavigateToDashboard()
        {
            if (_currentUser != null)
            {
                _navigationService.NavigateToDashboard(); // هذه ستعرض DashboardUserControl
                ShowDashboardStats = true;
            }
        }
        private void NavigateToAddPatients()
        {
            if (_currentUser != null)
            {
                // سنحتاج إلى طريقة في NavigationService للتعامل مع هذا
                // _navigationService.NavigateTo(typeof(AddPatientUserControl)); // أو اسم مشابه للدالة
                _navigationService.NavigateToView("AddPatient"); // اسم رمزي للواجهة، سنعرفه في NavigationService
                ShowDashboardStats = false;
            }
        }
        private void NavigateToEnterResults()
        {
            if (_currentUser != null)
            {
                // _navigationService.NavigateTo(typeof(EnterResultsView)); // أو اسم مشابه للدالة
                _navigationService.NavigateToView("EnterResults"); // اسم رمزي للواجهة
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
        private void NavigateToSettings()
        {
            ShowDashboardStats = false;
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

    public class NavigationItem : BaseViewModel // NavigationItem class remains the same
    {
        private bool _isSelected;
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public RelayCommand? Command { get; set; }
        public bool IsEnabled { get; set; } = true; // Default to true
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}