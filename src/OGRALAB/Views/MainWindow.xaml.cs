using System;
using System.Windows;
using OGRALAB.Models;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(User currentUser) : this()
        {
            _viewModel = new MainViewModel(currentUser);
            DataContext = _viewModel;
        }

        public MainWindow(MainViewModel viewModel) : this()
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            // Cleanup if needed
            base.OnClosed(e);
        }
    }
}
