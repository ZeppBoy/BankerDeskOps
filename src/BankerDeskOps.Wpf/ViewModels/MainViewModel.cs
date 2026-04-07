using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application window.
    /// Handles navigation between different views.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly ILogger<MainViewModel> _logger;

        [ObservableProperty]
        private ObservableObject? currentView;

        public MainViewModel(ILogger<MainViewModel> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Navigates to the Loans view.
        /// </summary>
        [RelayCommand]
        public void NavigateToLoans()
        {
            _logger.LogInformation("Navigating to Loans view");
        }

        /// <summary>
        /// Navigates to the Retail Accounts view.
        /// </summary>
        [RelayCommand]
        public void NavigateToRetailAccounts()
        {
            _logger.LogInformation("Navigating to Retail Accounts view");
        }
    }
}
