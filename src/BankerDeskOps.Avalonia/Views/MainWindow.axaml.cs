using Avalonia.Controls;
using BankerDeskOps.Avalonia.ViewModels;

namespace BankerDeskOps.Avalonia.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// Parameterless constructor required by the Avalonia AXAML design-time loader.
    /// At runtime the DI constructor below is always used instead.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel mainViewModel, LoansViewModel loansViewModel) : this()
    {
        DataContext = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

        // Default landing view — mirrors the WPF behaviour
        mainViewModel.CurrentView = loansViewModel;
        loansViewModel.LoadLoansCommand.Execute(null);
    }
}
