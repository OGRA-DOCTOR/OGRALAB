using System.Windows.Controls;
using OGRALAB.ViewModels;
using System; // For ArgumentNullException

namespace OGRALAB.Views
{
    /// <summary>
    /// Interaction logic for EnterResultsUserControl.xaml
    /// </summary>
    public partial class EnterResultsUserControl : UserControl
    {
        public EnterResultsUserControl()
        {
            InitializeComponent();
            // It's generally better to set DataContext via DI or NavigationService
            Loaded += (s, e) =>
            {
                if (DataContext is EnterResultsViewModel vm && vm.LoadDataCommand != null)
                {
                    if (vm.LoadDataCommand.CanExecute(null))
                    {
                        vm.LoadDataCommand.Execute(null);
                    }
                }
            };
            Unloaded += EnterResultsUserControl_Unloaded;
        }

        // Constructor to be used by DI/NavigationService
        public EnterResultsUserControl(EnterResultsViewModel viewModel) : this()
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        private void EnterResultsUserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Clean up ViewModel events if necessary
        }
    }
}