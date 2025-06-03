using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel? _viewModel;

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
            PasswordTextBox.TextChanged += PasswordTextBox_TextChanged;
            
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
            if (_viewModel != null && sender is PasswordBox passwordBox)
            {
                _viewModel.HandlePasswordChanged(sender, e);
            }
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel != null && sender is TextBox textBox)
            {
                _viewModel.Password = textBox.Text;
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
        }

        private void UpdatePasswordVisibility()
        {
            if (_viewModel != null)
            {
                if (_viewModel.IsPasswordVisible)
                {
                    PasswordTextBox.Text = _viewModel.Password;
                    PasswordBox.Visibility = Visibility.Collapsed;
                    PasswordTextBox.Visibility = Visibility.Visible;
                    PasswordTextBox.Focus();
                }
                else
                {
                    PasswordBox.Password = _viewModel.Password;
                    PasswordBox.Visibility = Visibility.Visible;
                    PasswordTextBox.Visibility = Visibility.Collapsed;
                    PasswordBox.Focus();
                }
            }
        }

        private void UpdateStatusMessage()
        {
            if (_viewModel != null)
            {
                StatusMessageBlock.Visibility = string.IsNullOrEmpty(_viewModel.StatusMessage) 
                    ? Visibility.Collapsed 
                    : Visibility.Visible;
            }
        }

        private void UpdateLoadingIndicator()
        {
            if (_viewModel != null)
            {
                LoadingProgressBar.Visibility = _viewModel.IsLoggingIn 
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
            if (PasswordTextBox != null)
            {
                PasswordTextBox.TextChanged -= PasswordTextBox_TextChanged;
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
