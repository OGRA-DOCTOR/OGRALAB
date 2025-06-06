using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; // *** إضافة مهمة لاستخدام MessageBox ***
using Microsoft.Extensions.DependencyInjection;
using OGRALAB.Commands;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Views;

namespace OGRALAB.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _selectedUsername = string.Empty;
        private bool _isPasswordVisible = false;
        private bool _rememberMe = false;
        private bool _isLoggingIn = false; // ستبقى هذه الخاصية لكن لن نستخدمها لإظهار شريط التحميل
        private string _statusMessage = string.Empty; // ستبقى هذه الخاصية لكن لن نستخدمها لرسائل المصادقة أو النجاح
        private ObservableCollection<string> _recentUsernames = new();

        public LoginViewModel(IAuthenticationService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            ExitCommand = new RelayCommand(Exit);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await LoadRecentUsernamesAsync();
        }

        #region Properties
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    LoginCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    LoginCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string SelectedUsername
        {
            get => _selectedUsername;
            set
            {
                if (SetProperty(ref _selectedUsername, value) && !string.IsNullOrEmpty(value))
                {
                    Username = value;
                    _ = LoadUserSettingsForUsernameAsync(value);
                }
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                SetProperty(ref _isPasswordVisible, value);
            }
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public bool IsLoggingIn // ستبقى هذه الخاصية لكن لن يتم استخدامها لإظهار مؤشر تحميل مرئي
        {
            get => _isLoggingIn;
            set => SetProperty(ref _isLoggingIn, value);
        }

        public string StatusMessage // ستبقى هذه الخاصية لرسائل أخرى محتملة غير رسائل المصادقة
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<string> RecentUsernames
        {
            get => _recentUsernames;
            set => SetProperty(ref _recentUsernames, value);
        }
        #endregion

        #region Commands
        public AsyncRelayCommand LoginCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand TogglePasswordVisibilityCommand { get; }
        #endregion

        #region Methods
        private bool CanLogin() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsLoggingIn;

        private async Task LoginAsync()
        {
            IsLoggingIn = true; // سنبقي هذا لتعطيل زر تسجيل الدخول أثناء العملية
            // StatusMessage = "Authenticating..."; // --- تم التعليق/الحذف: لن نعرض هذه الرسالة ---

            try
            {
                var user = await _authService.AuthenticateAsync(Username, Password);
                if (user != null)
                {
                    // StatusMessage = "Login successful!"; // --- تم التعليق/الحذف: لن نعرض هذه الرسالة ---
                    await _authService.UpdateLastLoginAsync(Username);
                    await _authService.SaveUserSettingsAsync(Username, RememberMe);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            if (App.AppHost?.Services == null)
                            {
                                throw new InvalidOperationException("Application Host or Services not initialized.");
                            }
                            var mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
                            var mainVm = mainWindow.DataContext as MainViewModel;
                            if (mainVm != null)
                            {
                                mainVm.InitializeForUser(user);
                            }
                            else
                            {
                                throw new InvalidOperationException("MainViewModel not found in MainWindow's DataContext.");
                            }
                            Application.Current.MainWindow = mainWindow;
                            mainWindow.Show();
                            var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                            loginWindow?.Close();
                        }
                        catch (Exception ex)
                        {
                            // رسائل الخطأ الحرجة مثل هذه يمكن أن تظل كـ MessageBox أو StatusMessage حسب الرغبة
                            MessageBox.Show($"Error opening main window: {ex.Message}\n{ex.StackTrace}", "خطأ فادح",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                            StatusMessage = "فشل في فتح النافذة الرئيسية."; // كمثال لرسالة داخلية
                        }
                    });
                }
                else
                {
                    // *** التغيير الرئيسي هنا: استخدام MessageBox للخطأ ***
                    MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة.",
                                    "خطأ في تسجيل الدخول",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    Password = string.Empty; // مسح كلمة المرور
                    // StatusMessage = "Invalid username or password."; // --- تم استبداله بـ MessageBox ---
                }
            }
            catch (Exception ex)
            {
                // لأخطاء الاتصال أو الأخطاء غير المتوقعة الأخرى
                MessageBox.Show($"فشل تسجيل الدخول: {ex.Message}", "خطأ في النظام", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Login exception: {ex}");
                // StatusMessage = $"Login failed: {ex.Message}"; // يمكن استبداله أو إبقاؤه إذا كان هناك مكان آخر يعرضه
            }
            finally
            {
                IsLoggingIn = false; // إعادة تمكين زر تسجيل الدخول
            }
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private async Task LoadRecentUsernamesAsync()
        {
            try
            {
                var usernames = await _authService.GetRecentUsernamesAsync();
                RecentUsernames = new ObservableCollection<string>(usernames);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent usernames: {ex.Message}");
            }
        }

        private async Task LoadUserSettingsForUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                RememberMe = false;
                return;
            }
            try
            {
                var settings = await _authService.GetUserSettingsAsync(username);
                if (settings != null && settings.RememberMe && settings.RememberMeExpiry > DateTime.UtcNow)
                {
                    RememberMe = true;
                }
                else
                {
                    RememberMe = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user settings for {username}: {ex.Message}");
                RememberMe = false;
            }
        }
        #endregion
    }
}