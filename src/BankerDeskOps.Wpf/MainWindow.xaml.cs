using System.Windows;
using BankerDeskOps.Wpf.ViewModels;
using BankerDeskOps.Wpf.Views;

namespace BankerDeskOps.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel;
        private readonly LoansView _loansView;
        private readonly LoansViewModel _loansViewModel;
        private readonly RetailAccountsView _retailAccountsView;
        private readonly RetailAccountsViewModel _retailAccountsViewModel;

        public MainWindow(
            MainViewModel mainViewModel,
            LoansView loansView,
            LoansViewModel loansViewModel,
            RetailAccountsView retailAccountsView,
            RetailAccountsViewModel retailAccountsViewModel)
        {
            InitializeComponent();

            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _loansView = loansView ?? throw new ArgumentNullException(nameof(loansView));
            _loansViewModel = loansViewModel ?? throw new ArgumentNullException(nameof(loansViewModel));
            _retailAccountsView = retailAccountsView ?? throw new ArgumentNullException(nameof(retailAccountsView));
            _retailAccountsViewModel = retailAccountsViewModel ?? throw new ArgumentNullException(nameof(retailAccountsViewModel));

            DataContext = _mainViewModel;

            // Load default view
            _loansView.DataContext = _loansViewModel;
            _mainViewModel.CurrentView = _loansViewModel;
            _loansViewModel.LoadLoansCommand.Execute(null);
        }
    }
}
