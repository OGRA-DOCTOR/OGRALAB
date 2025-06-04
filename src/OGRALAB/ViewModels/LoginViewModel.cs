using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private bool _isLoggingIn = false;
        private string _statusMessage = string.Empty;
        private ObservableCollection<string> _recentUsernames = new();
        public LoginViewModel(IAuthenticationService authService)
        {
            _authService = authService;
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            ExitCommand = new RelayCommand(Exit);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            UsernameSelectionChangedCommand = new RelayCommand<string>(OnUsernameSelectionChanged);

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await LoadRecentUsernamesAsync();
            await LoadUserSettingsAsync();
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
                    _ = LoadUserSettingsForUsernameAsync(value);
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
                if (SetProperty(ref _selectedUsername, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Username = value;
                    }
                }
            }
        }
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }
        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set => SetProperty(ref _isLoggingIn, value);
        }
        public string StatusMessage
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
        public RelayCommand<string> UsernameSelectionChangedCommand { get; }
        #endregion
        #region Methods
        private bool CanLogin() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsLoggingIn;
        private async Task LoginAsync()
        {
            try
            {
                IsLoggingIn = true;
                StatusMessage = "Authenticating...";
                var user = await _authService.AuthenticateAsync(Username, Password);
                if (user != null)
                {
                    StatusMessage = "Login successful!";

                    // Update last login
                    await _authService.UpdateLastLoginAsync(Username);

                    // Save settings if remember me is checked
                    if (RememberMe)
                    {
                        await _authService.SaveUserSettingsAsync(Username, RememberMe);
                    }
                    // Navigate to main window - FIXED VERSION
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"Creating MainWindow for user: {user.Username}");

                            var mainWindow = new MainWindow(user);

                            // تعيين النافذة الرئيسية بشكل صحيح - هذا هو الإصلاح الرئيسي
                            Application.Current.MainWindow = mainWindow;

                            System.Diagnostics.Debug.WriteLine("MainWindow assigned to Application.Current.MainWindow");

                            mainWindow.Show();

                            System.Diagnostics.Debug.WriteLine($"MainWindow.Show() called - IsVisible: {mainWindow.IsVisible}");

                            // إغلاق نافذة تسجيل الدخول بعد التأكد من فتح النافذة الرئيسية
                            var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                            if (loginWindow != null)
                            {
                                System.Diagnostics.Debug.WriteLine("Closing LoginWindow");
                                loginWindow.Close();
                            }

                            System.Diagnostics.Debug.WriteLine($"Navigation completed - Active windows: {Application.Current.Windows.Count}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in navigation: {ex.Message}");
                            MessageBox.Show($"Error opening main window: {ex.Message}", "Error",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                            StatusMessage = "Failed to open main window. Please try again.";
                        }
                    });
                }
                else
                {
                    StatusMessage = "Invalid username or password.";
                    Password = string.Empty;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Login failed: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Login exception: {ex}");
            }
            finally
            {
                IsLoggingIn = false;
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
        private void OnUsernameSelectionChanged(string? username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                Username = username;
                _ = LoadUserSettingsForUsernameAsync(username);
            }
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
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error loading recent usernames: {ex.Message}");
            }
        }
        private async Task LoadUserSettingsAsync()
        {
            // Check if there's a default username to load
            if (RecentUsernames.Count > 0)
            {
                var lastUsername = RecentUsernames.First();
                await LoadUserSettingsForUsernameAsync(lastUsername);
            }
        }
        private async Task LoadUserSettingsForUsernameAsync(string username)
        {
            try
            {
                var settings = await _authService.GetUserSettingsAsync(username);
                if (settings != null && settings.RememberMeExpiry > DateTime.UtcNow)
                {
                    RememberMe = settings.RememberMe;
                    if (settings.RememberMe)
                    {
                        Username = settings.Username;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error loading user settings: {ex.Message}");
            }
        }
        public void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                Password = passwordBox.Password;
            }
        }
        #endregion
    }
}