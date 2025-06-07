using System.Collections.ObjectModel;
using System.Windows.Controls;
using OGRALAB.Commands;
using OGRALAB.Models;
using OGRALAB.Services;
using System;
using System.Linq;
using System.Windows.Input; // *** إضافة لاستخدام ICommand ***
using System.Windows.Media; // *** إضافة لاستخدام SolidColorBrush ***
using System.Windows;     // *** إضافة لاستخدام Application.Current.Resources ***

namespace OGRALAB.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private User? _currentUser;
        private readonly INavigationService _navigationService;
        private string _welcomeMessage = string.Empty;
        private UserControl? _currentContent;
        private NavigationItem? _selectedMenuItem;

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _navigationService.ContentChanged += OnContentChanged;
        }

        public void InitializeForUser(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            WelcomeMessage = $"مرحباً، {_currentUser.FullName}!";
            InitializeCommands(); // Commands should be initialized before MenuItems
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

        // *** الخصائص الجديدة للألوان من مقترح الوكيل (تتطلب تعريف الألوان في XAML) ***
        public SolidColorBrush DashboardColor => (SolidColorBrush)Application.Current.Resources["DashboardColor"];
        public SolidColorBrush AddPatientsColor => (SolidColorBrush)Application.Current.Resources["AddPatientsColor"];
        public SolidColorBrush EnterResultsColor => (SolidColorBrush)Application.Current.Resources["EnterResultsColor"];
        public SolidColorBrush PreviewReportsColor => (SolidColorBrush)Application.Current.Resources["PreviewReportsColor"];
        public SolidColorBrush SettingsColor => (SolidColorBrush)Application.Current.Resources["SettingsColor"];
        public SolidColorBrush LogoutColor => (SolidColorBrush)Application.Current.Resources["LogoutColor"];
        #endregion

        #region الأوامر
        public ICommand DashboardCommand { get; private set; } = null!;
        public ICommand AddPatientsCommand { get; private set; } = null!;
        public ICommand EnterResultsCommand { get; private set; } = null!;
        public ICommand PreviewReportsCommand { get; private set; } = null!;
        public ICommand LogoutCommand { get; private set; } = null!;
        public ICommand SettingsCommand { get; private set; } = null!; // أمر الإعدادات
        #endregion

        #region طرق التهيئة
        private void InitializeCommands()
        {
            DashboardCommand = new RelayCommand(NavigateToDashboard);
            AddPatientsCommand = new RelayCommand(NavigateToAddPatients);
            EnterResultsCommand = new RelayCommand(NavigateToEnterResults);
            PreviewReportsCommand = new RelayCommand(NavigateToPreviewReports); // تفعيل أمر التقارير
            LogoutCommand = new RelayCommand(Logout);
            SettingsCommand = new RelayCommand(NavigateToSettings, () => false); // تعطيل مؤقت
        }

        private void InitializeMenuItems()
        {
            MenuItems.Clear();
            if (_currentUser == null) return;

            // استخدام التصميم الجديد المقترح من الوكيل
            MenuItems.Add(new NavigationItem { Name = "لوحة المعلومات", Icon = "\uE80F", Color = DashboardColor, Command = DashboardCommand, IsEnabled = true, IsSelected = true });
            MenuItems.Add(new NavigationItem { Name = "إدخال المرضى", Icon = "\uE716", Color = AddPatientsColor, Command = AddPatientsCommand, IsEnabled = true });
            MenuItems.Add(new NavigationItem { Name = "إدخال النتائج", Icon = "\uE9F9", Color = EnterResultsColor, Command = EnterResultsCommand, IsEnabled = true });

            // *** تم التعديل هنا ***
            // تم تغيير اسم العنصر من "معاينة التقارير" إلى "معاينة وطباعة التقارير"
            MenuItems.Add(new NavigationItem { Name = "معاينة وطباعة التقارير", Icon = "\uE8A5", Color = PreviewReportsColor, Command = PreviewReportsCommand, IsEnabled = true });

            MenuItems.Add(new NavigationItem { Name = "الإعدادات", Icon = "\uE713", Color = SettingsColor, Command = SettingsCommand, IsEnabled = false }); // تعطيل مؤقت
            MenuItems.Add(new NavigationItem { Name = "تسجيل الخروج", Icon = "\uE7E8", Color = LogoutColor, Command = LogoutCommand, IsEnabled = true });

            SelectedMenuItem = MenuItems.FirstOrDefault(m => m.IsSelected);
        }
        #endregion

        #region طرق التنقل
        private void NavigateToDashboard() => _navigationService.NavigateToDashboard();
        private void NavigateToAddPatients() => _navigationService.NavigateToView("AddPatient");
        private void NavigateToEnterResults() => _navigationService.NavigateToView("EnterResults");
        private void NavigateToPreviewReports() => _navigationService.NavigateToReports(); // استدعاء الدالة الجديدة
        private void NavigateToSettings() { /* لا يتم عمل شيء حاليًا */ }
        private void Logout() => _navigationService.Logout();
        #endregion
    }

    // تعديل بسيط على NavigationItem ليطابق مقترح الوكيل
    public class NavigationItem : BaseViewModel
    {
        private bool _isSelected;
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public SolidColorBrush Color { get; set; } = Brushes.Black;
        public ICommand? Command { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}