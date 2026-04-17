using Avalonia.Controls;
using BankerDeskOps.Avalonia.ViewModels;

namespace BankerDeskOps.Avalonia.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    public LoginWindow(LoginViewModel loginViewModel) : this()
    {
        DataContext = loginViewModel ?? throw new ArgumentNullException(nameof(loginViewModel));
        loginViewModel.LoginSucceeded += Close;
    }
}
