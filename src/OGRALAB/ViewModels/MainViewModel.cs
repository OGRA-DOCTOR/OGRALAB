using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using OGRALAB.Commands;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Views;

namespace OGRALAB.ViewModels
{
    /// <summary>
    /// نموذج عرض النافذة الرئيسية مع قائمة التنقل الجانبية
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private readonly INavigationService _navigationService;
        private string _welcomeMessage = string.Empty;
        private UserControl? _currentContent;
        private NavigationItem? _selectedMenuItem;

        public MainViewModel(User currentUser, INavigationService navigationService)
        {
            _currentUser = currentUser;
            _navigationService = navigationService;
            
            InitializeCommands();
            InitializeMenuItems();
            InitializeNavigation();
            
            WelcomeMessage = $"مرحباً، {_currentUser.FullName}!";
        }

        #region خصائص

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public User CurrentUser => _currentUser;

        /// <summary>
        /// المحتوى الحالي المعروض
        /// </summary>
        public UserControl? CurrentContent
        {
            get => _currentContent;
            set => SetProperty(ref _currentContent, value);
        }

        /// <summary>
        /// عنصر القائمة المحدد حالياً
        /// </summary>
        public NavigationItem? SelectedMenuItem
        {
            get => _selectedMenuItem;
            set 
            { 
                SetProperty(ref _selectedMenuItem, value);
                if (value != null && value.IsEnabled)
                {
                    value.Command?.Execute(null);
                }
            }
        }

        /// <summary>
        /// عناصر قائمة التنقل
        /// </summary>
        public ObservableCollection<NavigationItem> MenuItems { get; private set; } = new ObservableCollection<NavigationItem>();

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

        /// <summary>
        /// تهيئة الأوامر
        /// </summary>
        private void InitializeCommands()
        {
            DashboardCommand = new RelayCommand(NavigateToDashboard);
            AddPatientsCommand = new RelayCommand(NavigateToAddPatients, () => false); // معطل
            EnterResultsCommand = new RelayCommand(NavigateToEnterResults, () => false); // معطل
            PreviewReportsCommand = new RelayCommand(NavigateToPreviewReports, () => false); // معطل
            SearchModifyCommand = new RelayCommand(NavigateToSearchModify, () => false); // معطل
            SettingsCommand = new RelayCommand(NavigateToSettings, () => false); // معطل
            LogoutCommand = new RelayCommand(Logout);
        }

        /// <summary>
        /// تهيئة عناصر القائمة
        /// </summary>
        private void InitializeMenuItems()
        {
            MenuItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem 
                { 
                    Icon = "📊", 
                    Title = "لوحة المعلومات", 
                    Command = DashboardCommand, 
                    IsEnabled = true,
                    IsSelected = true 
                },
                new NavigationItem 
                { 
                    Icon = "👥", 
                    Title = "إدخال المرضى", 
                    Command = AddPatientsCommand, 
                    IsEnabled = false 
                },
                new NavigationItem 
                { 
                    Icon = "📝", 
                    Title = "إدخال النتائج", 
                    Command = EnterResultsCommand, 
                    IsEnabled = false 
                },
                new NavigationItem 
                { 
                    Icon = "📄", 
                    Title = "معاينة وطباعة التقارير", 
                    Command = PreviewReportsCommand, 
                    IsEnabled = false 
                },
                new NavigationItem 
                { 
                    Icon = "🔍", 
                    Title = "البحث والتعديل", 
                    Command = SearchModifyCommand, 
                    IsEnabled = false 
                },
                new NavigationItem 
                { 
                    Icon = "⚙️", 
                    Title = "الإعدادات", 
                    Command = SettingsCommand, 
                    IsEnabled = false 
                },
                new NavigationItem 
                { 
                    Icon = "🚪", 
                    Title = "تسجيل الخروج", 
                    Command = LogoutCommand, 
                    IsEnabled = true 
                }
            };

            // تحديد العنصر الافتراضي
            _selectedMenuItem = MenuItems[0];
        }

        /// <summary>
        /// تهيئة نظام التنقل
        /// </summary>
        private void InitializeNavigation()
        {
            _navigationService.ContentChanged += (sender, content) =>
            {
                CurrentContent = content;
            };

            // التنقل إلى لوحة المعلومات افتراضياً
            _navigationService.NavigateToDashboard();
        }

        #endregion

        #region طرق التنقل

        /// <summary>
        /// التنقل إلى لوحة المعلومات
        /// </summary>
        private void NavigateToDashboard()
        {
            _navigationService.NavigateToDashboard();
        }

        /// <summary>
        /// التنقل إلى إدخال المرضى (معطل)
        /// </summary>
        private void NavigateToAddPatients()
        {
            // معطل حالياً
        }

        /// <summary>
        /// التنقل إلى إدخال النتائج (معطل)
        /// </summary>
        private void NavigateToEnterResults()
        {
            // معطل حالياً
        }

        /// <summary>
        /// التنقل إلى معاينة التقارير (معطل)
        /// </summary>
        private void NavigateToPreviewReports()
        {
            // معطل حالياً
        }

        /// <summary>
        /// التنقل إلى البحث والتعديل (معطل)
        /// </summary>
        private void NavigateToSearchModify()
        {
            // معطل حالياً
        }

        /// <summary>
        /// التنقل إلى الإعدادات (معطل)
        /// </summary>
        private void NavigateToSettings()
        {
            // معطل حالياً
        }

        /// <summary>
        /// تسجيل الخروج
        /// </summary>
        private void Logout()
        {
            _navigationService.Logout();
        }

        #endregion
    }

    /// <summary>
    /// عنصر في قائمة التنقل
    /// </summary>
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
