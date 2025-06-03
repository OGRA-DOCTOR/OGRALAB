using System.Windows;
using OGRALAB.Commands;
using OGRALAB.Models;
using OGRALAB.Views;

namespace OGRALAB.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private string _welcomeMessage = string.Empty;

        public MainViewModel(User currentUser)
        {
            _currentUser = currentUser;
            LogoutCommand = new RelayCommand(Logout);
            ExitCommand = new RelayCommand(Exit);
            
            WelcomeMessage = $"Welcome, {_currentUser.FullName}!";
        }

        #region Properties

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public User CurrentUser => _currentUser;

        #endregion

        #region Commands

        public RelayCommand LogoutCommand { get; }
        public RelayCommand ExitCommand { get; }

        #endregion

        #region Methods

        private void Logout()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                
                // Close main window
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            });
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        #endregion
    }
}
