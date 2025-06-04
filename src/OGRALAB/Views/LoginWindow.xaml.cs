using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel? _viewModel;
        private bool _isUpdatingPassword;

        public LoginWindow()
        {
            InitializeComponent();
            Loaded += LoginWindow_Loaded;
        }

        public LoginWindow(LoginViewModel viewModel) : this()
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to username field
            UsernameComboBox.Focus();

            // Handle password box events
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;

            // Handle Enter key press
            KeyDown += LoginWindow_KeyDown;

            // Subscribe to property changes if ViewModel is available
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && !_isUpdatingPassword && sender is PasswordBox passwordBox)
            {
                _isUpdatingPassword = true;
                try
                {
                    _viewModel.Password = passwordBox.Password;
                }
                finally
                {
                    _isUpdatingPassword = false;
                }
            }
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel != null && !_isUpdatingPassword && sender is TextBox textBox)
            {
                _isUpdatingPassword = true;
                try
                {
                    _viewModel.Password = textBox.Text;
                    if (!_viewModel.IsPasswordVisible)
                    {
                        PasswordBox.Password = textBox.Text;
                    }
                }
                finally
                {
                    _isUpdatingPassword = false;
                }
            }
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_viewModel?.LoginCommand.CanExecute(null) == true)
                {
                    _viewModel.LoginCommand.Execute(null);
                }
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoginViewModel.IsPasswordVisible))
            {
                UpdatePasswordVisibility();
            }
            else if (e.PropertyName == nameof(LoginViewModel.StatusMessage))
            {
                UpdateStatusMessage();
            }
            else if (e.PropertyName == nameof(LoginViewModel.IsLoggingIn))
            {
                UpdateLoadingIndicator();
            }
            else if (e.PropertyName == nameof(LoginViewModel.Password) && !_isUpdatingPassword)
            {
                UpdatePasswordControls();
            }
        }

        private void UpdatePasswordControls()
        {
            if (_viewModel == null) return;

            _isUpdatingPassword = true;
            try
            {
                if (_viewModel.IsPasswordVisible)
                {
                    PasswordTextBox.Text = _viewModel.Password;
                }
                else
                {
                    PasswordBox.Password = _viewModel.Password;
                }
            }
            finally
            {
                _isUpdatingPassword = false;
            }
        }

        private void UpdatePasswordVisibility()
        {
            if (_viewModel == null) return;

            _isUpdatingPassword = true;
            try
            {
                if (_viewModel.IsPasswordVisible)
                {
                    PasswordTextBox.Text = PasswordBox.Password;
                    PasswordBox.Visibility = Visibility.Collapsed;
                    PasswordTextBox.Visibility = Visibility.Visible;
                    PasswordTextBox.Focus();
                }
                else
                {
                    PasswordBox.Password = PasswordTextBox.Text;
                    PasswordBox.Visibility = Visibility.Visible;
                    PasswordTextBox.Visibility = Visibility.Collapsed;
                    PasswordBox.Focus();
                }
            }
            finally
            {
                _isUpdatingPassword = false;
            }
        }

        private void UpdateStatusMessage()
        {
            if (_viewModel != null)
            {
                bool hasMessage = !string.IsNullOrEmpty(_viewModel.StatusMessage);
                StatusMessageBorder.Visibility = hasMessage ? Visibility.Visible : Visibility.Collapsed;

                // Change color based on message type
                if (hasMessage)
                {
                    if (_viewModel.StatusMessage.Contains("successful") || _viewModel.StatusMessage.Contains("success"))
                    {
                        StatusMessageBorder.Background = (SolidColorBrush)FindResource("SuccessColor");
                    }
                    else if (_viewModel.StatusMessage.Contains("Authenticating"))
                    {
                        StatusMessageBorder.Background = (SolidColorBrush)FindResource("SecondaryColor");
                    }
                    else
                    {
                        StatusMessageBorder.Background = (SolidColorBrush)FindResource("ErrorColor");
                    }
                }
            }
        }

        private void UpdateLoadingIndicator()
        {
            if (_viewModel != null)
            {
                LoadingBorder.Visibility = _viewModel.IsLoggingIn
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Cleanup
            if (PasswordBox != null)
            {
                PasswordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            }
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            KeyDown -= LoginWindow_KeyDown;

            base.OnClosed(e);
        }
    }
}