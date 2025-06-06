using System;
using System.Windows.Controls;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class AddPatientUserControl : UserControl
    {
        public AddPatientUserControl() // Constructor might not need ViewModel if DataContext is set by NavigationService
        {
            InitializeComponent();

            // It's better if the NavigationService (or DI container) sets the DataContext.
            // If AddPatientViewModel needs parameters, this constructor won't work well with DI directly for the view.
            // However, if AddPatientViewModel has a parameterless constructor OR 
            // if you are creating the ViewModel instance somewhere else and setting it, this is fine.

            Loaded += (s, e) =>
            {
                if (DataContext is AddPatientViewModel vm && vm.LoadDataCommand != null)
                {
                    if (vm.LoadDataCommand.CanExecute(null))
                    {
                        vm.LoadDataCommand.Execute(null);
                    }
                }
            };

            // Unloaded event to clean up if necessary
            Unloaded += AddPatientUserControl_Unloaded;
        }

        // This constructor is preferred if the NavigationService or DI will provide the ViewModel instance
        public AddPatientUserControl(AddPatientViewModel viewModel) : this() // Calls the parameterless constructor first
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            // Event subscriptions that were previously here for DialogResult/Close are handled differently now
            // e.g., ViewModel might navigate away upon save/cancel
        }

        private void AddPatientUserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Clean up ViewModel events if they were subscribed and could cause memory leaks
            // For example, if the ViewModel has events that this UserControl subscribes to:
            // if (DataContext is AddPatientViewModel vm)
            // {
            //     vm.SomeEvent -= Vm_SomeEvent;
            // }
        }
    }
}