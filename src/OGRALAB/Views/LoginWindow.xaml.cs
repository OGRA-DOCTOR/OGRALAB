using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OGRALAB.ViewModels; // تأكد أن هذا السطر موجود لاستخدام LoginViewModel

namespace OGRALAB.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
                UpdatePasswordFieldsVisibilityAndFocus(vm.IsPasswordVisible, true);
            }
            PasswordBoxField.PasswordChanged += PasswordBoxField_PasswordChanged;
            KeyDown += LoginWindow_KeyDown;
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
            {
                if (vm.LoginCommand.CanExecute(null))
                {
                    vm.LoginCommand.Execute(null);
                }
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                switch (e.PropertyName)
                {
                    case nameof(LoginViewModel.IsPasswordVisible):
                        UpdatePasswordFieldsVisibilityAndFocus(vm.IsPasswordVisible, false);
                        break;
                    case nameof(LoginViewModel.Password):
                        if (!PasswordBoxField.IsKeyboardFocusWithin && !PasswordTextBoxField.IsKeyboardFocusWithin)
                        {
                            PasswordBoxField.Password = vm.Password;
                            PasswordTextBoxField.Text = vm.Password;
                        }
                        else if (vm.IsPasswordVisible && PasswordTextBoxField.Text != vm.Password)
                        {
                            PasswordTextBoxField.Text = vm.Password;
                        }
                        else if (!vm.IsPasswordVisible && PasswordBoxField.Password != vm.Password)
                        {
                            PasswordBoxField.Password = vm.Password;
                        }
                        break;
                    case nameof(LoginViewModel.StatusMessage):
                        // UpdateStatusMessageDisplay(vm); // *** تم التعليق: لم نعد نعرض StatusMessageBorder ***
                        break;
                    case nameof(LoginViewModel.IsLoggingIn):
                        // UpdateLoadingIndicatorDisplay(vm); // *** تم التعليق: لم نعد نعرض LoadingBorder ***
                        break;
                }
            }
        }

        private void UpdatePasswordFieldsVisibilityAndFocus(bool isPasswordVisible, bool isInitialLoad)
        {
            if (isPasswordVisible)
            {
                PasswordBoxField.Visibility = Visibility.Collapsed;
                PasswordTextBoxField.Visibility = Visibility.Visible;
                PasswordTextBoxField.Focus();
                PasswordTextBoxField.CaretIndex = PasswordTextBoxField.Text?.Length ?? 0;
            }
            else
            {
                PasswordTextBoxField.Visibility = Visibility.Collapsed;
                PasswordBoxField.Visibility = Visibility.Visible;
                if (!isInitialLoad || !string.IsNullOrEmpty(UsernameComboBox.Text))
                {
                    PasswordBoxField.Focus();
                }
                else if (isInitialLoad && string.IsNullOrEmpty(UsernameComboBox.Text))
                {
                    UsernameComboBox.Focus();
                }
            }
        }

        private void PasswordBoxField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
            {
                if (!vm.IsPasswordVisible)
                {
                    if (vm.Password != pb.Password)
                    {
                        vm.Password = pb.Password;
                    }
                }
            }
        }

        // *** الدالة التالية لم تعد ضرورية ويمكن حذفها بالكامل أو تعليقها ***
        /*
        private void UpdateStatusMessageDisplay(LoginViewModel vm)
        {
            // هذا الكود كان يتحكم في StatusMessageBorder
            // بما أن StatusMessageBorder مخفي دائمًا الآن في XAML، فهذه الدالة لم تعد تفعل شيئًا مرئيًا
            // bool hasMessage = !string.IsNullOrEmpty(vm.StatusMessage);
            // StatusMessageBorder.Visibility = hasMessage ? Visibility.Visible : Visibility.Collapsed;
            // if (hasMessage)
            // {
            //     if (vm.StatusMessage.Contains("successful", StringComparison.OrdinalIgnoreCase))
            //         StatusMessageBorder.Background = (System.Windows.Media.Brush)FindResource("SuccessColor");
            //     else if (vm.StatusMessage.Contains("Authenticating", StringComparison.OrdinalIgnoreCase))
            //         StatusMessageBorder.Background = (System.Windows.Media.Brush)FindResource("SecondaryColor");
            //     else
            //         StatusMessageBorder.Background = (System.Windows.Media.Brush)FindResource("ErrorColor");
            // }
        }
        */

        // *** الدالة التالية لم تعد ضرورية ويمكن حذفها بالكامل أو تعليقها ***
        /*
        private void UpdateLoadingIndicatorDisplay(LoginViewModel vm)
        {
            // هذا الكود كان يتحكم في LoadingBorder
            // بما أن LoadingBorder مخفي دائمًا الآن في XAML، فهذه الدالة لم تعد تفعل شيئًا مرئيًا
            // LoadingBorder.Visibility = vm.IsLoggingIn ? Visibility.Visible : Visibility.Collapsed;
        }
        */

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
            }
            PasswordBoxField.PasswordChanged -= PasswordBoxField_PasswordChanged;
            KeyDown -= LoginWindow_KeyDown;
            base.OnClosed(e);
        }
    }
}