using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Enums;
namespace OGRALAB.ViewModels
{
    /// <summary>
    /// نموذج عرض إدارة المستخدمين
    /// يتعامل مع جميع عمليات CRUD للمستخدمين والصلاحيات
    /// </summary>
    public class UserManagementViewModel : BaseViewModel
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IAuthenticationService _authenticationService;
        public UserManagementViewModel(IUserManagementService userManagementService, IAuthenticationService authenticationService)
        {
            _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            Users = new ObservableCollection<User>();

            InitializeCommands();
            LoadUsersAsync();
        }
        #region Properties
        /// <summary>
        /// قائمة المستخدمين
        /// </summary>
        public ObservableCollection<User> Users { get; set; }
        private User? _selectedUser;
        /// <summary>
        /// المستخدم المحدد حالياً
        /// </summary>
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                if (value != null)
                {
                    LoadUserForEditing(value);
                }
                else
                {
                    ClearForm();
                }
                UpdateCommandStates();
            }
        }
        private string _username = "";
        /// <summary>
        /// اسم المستخدم
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                UpdateCommandStates();
            }
        }
        private string _fullName = "";
        /// <summary>
        /// الاسم الكامل
        /// </summary>
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }
        private string _email = "";
        /// <summary>
        /// البريد الإلكتروني
        /// </summary>
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        private string _password = "";
        /// <summary>
        /// كلمة المرور
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                UpdateCommandStates();
            }
        }
        private string _confirmPassword = "";
        /// <summary>
        /// تأكيد كلمة المرور
        /// </summary>
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                UpdateCommandStates();
            }
        }
        private UserRole _selectedRole = UserRole.User;
        /// <summary>
        /// الصلاحيات المحددة
        /// </summary>
        public UserRole SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }
        private bool _isLoading;
        /// <summary>
        /// حالة التحميل
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                UpdateCommandStates();
            }
        }
        private string _statusMessage = "";
        /// <summary>
        /// رسالة الحالة
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        /// <summary>
        /// قائمة أنواع الصلاحيات
        /// </summary>
        public Array UserRoles => Enum.GetValues(typeof(UserRole));
        /// <summary>
        /// هل كلمات المرور متطابقة؟
        /// </summary>
        public bool ArePasswordsMatching => !string.IsNullOrEmpty(Password) && Password == ConfirmPassword;
        /// <summary>
        /// هل كلمة المرور صحيحة؟
        /// </summary>
        public bool IsPasswordValid => _userManagementService.ValidatePassword(Password);
        #endregion
        #region Commands
        public ICommand LoadUsersCommand { get; private set; } = null!;
        public ICommand AddUserCommand { get; private set; } = null!;
        public ICommand UpdateUserCommand { get; private set; } = null!;
        public ICommand DeleteUserCommand { get; private set; } = null!;
        public ICommand ChangePasswordCommand { get; private set; } = null!;
        public ICommand RefreshCommand { get; private set; } = null!;
        public ICommand CloseCommand { get; private set; } = null!;
        public ICommand ClearFormCommand { get; private set; } = null!;
        #endregion
        #region Methods
        /// <summary>
        /// تهيئة الأوامر
        /// </summary>
        private void InitializeCommands()
        {
            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync, CanAddUser);
            UpdateUserCommand = new AsyncRelayCommand(UpdateUserAsync, CanUpdateUser);
            DeleteUserCommand = new AsyncRelayCommand(DeleteUserAsync, CanDeleteUser);
            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync, CanChangePassword);
            RefreshCommand = new AsyncRelayCommand(LoadUsersAsync);
            CloseCommand = new RelayCommand(CloseWindow);
            ClearFormCommand = new RelayCommand(ClearForm);
        }
        /// <summary>
        /// تحميل قائمة المستخدمين
        /// </summary>
        private async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل المستخدمين...";
                var users = await _userManagementService.GetAllUsersAsync();
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
                StatusMessage = $"تم تحميل {users.Count} مستخدم";
                await Task.Delay(2000);
                StatusMessage = "";
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "UserManagementViewModel.LoadUsersAsync");
                StatusMessage = "حدث خطأ أثناء تحميل المستخدمين";
                MessageBox.Show($"حدث خطأ أثناء تحميل المستخدمين:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// فحص إمكانية إضافة مستخدم جديد
        /// </summary>
        private bool CanAddUser()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   ArePasswordsMatching &&
                   IsPasswordValid;
        }
        /// <summary>
        /// إضافة مستخدم جديد
        /// </summary>
        private async Task AddUserAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري إضافة المستخدم...";
                var success = await _userManagementService.AddUserAsync(
                    Username, Password, SelectedRole,
                    string.IsNullOrWhiteSpace(FullName) ? null : FullName,
                    string.IsNullOrWhiteSpace(Email) ? null : Email);
                if (success)
                {
                    StatusMessage = "تم إضافة المستخدم بنجاح";
                    ClearForm();
                    await LoadUsersAsync();
                }
                else
                {
                    StatusMessage = "فشل في إضافة المستخدم";
                    MessageBox.Show("فشل في إضافة المستخدم. تأكد من أن اسم المستخدم غير موجود وأن كلمة المرور صحيحة.",
                                   "خطأ في الإضافة", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "UserManagementViewModel.AddUserAsync");
                StatusMessage = "حدث خطأ أثناء إضافة المستخدم";
                MessageBox.Show($"حدث خطأ أثناء إضافة المستخدم:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// فحص إمكانية تحديث المستخدم
        /// </summary>
        private bool CanUpdateUser()
        {
            return !IsLoading &&
                   SelectedUser != null &&
                   !string.IsNullOrWhiteSpace(Username);
        }
        /// <summary>
        /// تحديث بيانات المستخدم
        /// </summary>
        private async Task UpdateUserAsync()
        {
            if (SelectedUser == null) return;
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحديث بيانات المستخدم...";
                var success = await _userManagementService.UpdateUserAsync(
                    SelectedUser.Id, Username, SelectedRole,
                    string.IsNullOrWhiteSpace(FullName) ? null : FullName,
                    string.IsNullOrWhiteSpace(Email) ? null : Email);
                if (success)
                {
                    StatusMessage = "تم تحديث بيانات المستخدم بنجاح";
                    await LoadUsersAsync();
                }
                else
                {
                    StatusMessage = "فشل في تحديث بيانات المستخدم";
                    MessageBox.Show("فشل في تحديث بيانات المستخدم. تأكد من أن اسم المستخدم غير موجود.",
                                   "خطأ في التحديث", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "UserManagementViewModel.UpdateUserAsync");
                StatusMessage = "حدث خطأ أثناء تحديث بيانات المستخدم";
                MessageBox.Show($"حدث خطأ أثناء تحديث بيانات المستخدم:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// فحص إمكانية حذف المستخدم
        /// </summary>
        private bool CanDeleteUser()
        {
            return !IsLoading &&
                   SelectedUser != null &&
                   SelectedUser.Id != _authenticationService.CurrentUser?.Id; // منع حذف المستخدم الحالي
        }
        /// <summary>
        /// حذف المستخدم
        /// </summary>
        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null) return;
            var result = MessageBox.Show($"هل أنت متأكد من حذف المستخدم '{SelectedUser.Username}'؟\n\nهذا الإجراء لا يمكن التراجع عنه.",
                                        "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                IsLoading = true;
                StatusMessage = "جاري حذف المستخدم...";
                var success = await _userManagementService.DeleteUserAsync(SelectedUser.Id);
                if (success)
                {
                    StatusMessage = "تم حذف المستخدم بنجاح";
                    ClearForm();
                    await LoadUsersAsync();
                }
                else
                {
                    StatusMessage = "فشل في حذف المستخدم";
                    MessageBox.Show("لا يمكن حذف هذا المستخدم. قد يكون المسؤول الوحيد في النظام.",
                                   "لا يمكن الحذف", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "UserManagementViewModel.DeleteUserAsync");
                StatusMessage = "حدث خطأ أثناء حذف المستخدم";
                MessageBox.Show($"حدث خطأ أثناء حذف المستخدم:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// فحص إمكانية تغيير كلمة المرور
        /// </summary>
        private bool CanChangePassword()
        {
            return !IsLoading &&
                   SelectedUser != null &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   ArePasswordsMatching &&
                   IsPasswordValid;
        }
        /// <summary>
        /// تغيير كلمة مرور المستخدم
        /// </summary>
        private async Task ChangePasswordAsync()
        {
            if (SelectedUser == null) return;
            var result = MessageBox.Show($"هل أنت متأكد من تغيير كلمة مرور المستخدم '{SelectedUser.Username}'؟",
                                        "تأكيد تغيير كلمة المرور", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تغيير كلمة المرور...";
                var success = await _userManagementService.ChangePasswordAsync(SelectedUser.Id, Password);
                if (success)
                {
                    StatusMessage = "تم تغيير كلمة المرور بنجاح";
                    Password = "";
                    ConfirmPassword = "";
                }
                else
                {
                    StatusMessage = "فشل في تغيير كلمة المرور";
                    MessageBox.Show("فشل في تغيير كلمة المرور. تأكد من أن كلمة المرور الجديدة صحيحة.",
                                   "خطأ في تغيير كلمة المرور", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "UserManagementViewModel.ChangePasswordAsync");
                StatusMessage = "حدث خطأ أثناء تغيير كلمة المرور";
                MessageBox.Show($"حدث خطأ أثناء تغيير كلمة المرور:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// تحميل بيانات المستخدم للتعديل
        /// </summary>
        private void LoadUserForEditing(User user)
        {
            Username = user.Username;
            FullName = user.FullName ?? "";
            Email = user.Email ?? "";
            SelectedRole = user.Role;
            Password = "";
            ConfirmPassword = "";
        }
        /// <summary>
        /// مسح النموذج
        /// </summary>
        private void ClearForm()
        {
            SelectedUser = null;
            Username = "";
            FullName = "";
            Email = "";
            Password = "";
            ConfirmPassword = "";
            SelectedRole = UserRole.User;
        }
        /// <summary>
        /// تحديث حالة الأوامر
        /// </summary>
        private void UpdateCommandStates()
        {
            if (AddUserCommand is AsyncRelayCommand addCmd) addCmd.RaiseCanExecuteChanged();
            if (UpdateUserCommand is AsyncRelayCommand updateCmd) updateCmd.RaiseCanExecuteChanged();
            if (DeleteUserCommand is AsyncRelayCommand deleteCmd) deleteCmd.RaiseCanExecuteChanged();
            if (ChangePasswordCommand is AsyncRelayCommand changeCmd) changeCmd.RaiseCanExecuteChanged();
        }
        /// <summary>
        /// إغلاق النافذة
        /// </summary>
        private void CloseWindow()
        {
            var window = Application.Current.Windows.OfType<UserManagementWindow>().FirstOrDefault();
            window?.Close();
        }
        #endregion
    }
    /// <summary>
    /// أمر غير متزامن لتنفيذ المهام
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }
        public async void Execute(object? parameter)
        {
            if (_isExecuting) return;
            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}