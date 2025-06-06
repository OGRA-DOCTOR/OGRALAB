using System;
using System.Windows;
using OGRALAB.Models;     // For User
using OGRALAB.Services;   // For INavigationService
using OGRALAB.ViewModels; // For MainViewModel

namespace OGRALAB.Views
{
    public partial class MainWindow : Window
    {
        // The ViewModel is injected by DI through the constructor
        // The DataContext will be set to this injected ViewModel

        // Constructor for DI: receives MainViewModel (which in turn receives User and INavigationService via DI or setup)
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            // The MainViewModel should handle setting the current user if needed,
            // possibly through a service or by receiving it in its own constructor.
            Closed += MainWindow_Closed;
        }

        // This constructor might still be called by LoginViewModel if MainWindow is not resolved by DI properly.
        // However, the goal is for DI to create MainWindow.
        // If this constructor is used, INavigationService will be problematic as shown by the error.
        // The DI-friendly constructor `MainWindow(MainViewModel viewModel)` is preferred.
        /*
        public MainWindow(User currentUser) // This constructor creates dependencies manually
        {
            InitializeComponent();
            var navigationService = new NavigationService(); // Problematic: creates new instance without DI
                                                             // This instance won't have access to the correct ServiceProvider
                                                             // if it tries to use Application.Current.Resources["ServiceProvider"]
                                                             // and that resource wasn't set or was set with a different provider.

            // To fix this, if User must be passed, MainWindow should also take INavigationService via DI,
            // and then MainViewModel can be created.
            // OR, MainViewModel takes User and INavigationService via DI.

            var mainViewModel = new MainViewModel(currentUser, navigationService); // navigationService here is the new one
            DataContext = mainViewModel;
            Closed += MainWindow_Closed;
        }
        */


        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            // ViewModel cleanup can be handled by the ViewModel's own disposable pattern if needed,
            // or if it has specific resources to release.
            // For simple ViewModels, this might not be strictly necessary.
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        // OnClosed is fine as is.
        // protected override void OnClosed(EventArgs e)
        // {
        //     base.OnClosed(e);
        // }
    }
}