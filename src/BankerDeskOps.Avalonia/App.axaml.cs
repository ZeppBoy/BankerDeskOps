using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BankerDeskOps.Avalonia.Services;
using BankerDeskOps.Avalonia.ViewModels;
using BankerDeskOps.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Avalonia;

// 'Application' is disambiguated with global:: because BankerDeskOps.Application
// (the shared Application layer project) is visible in the parent namespace.
public partial class App : global::Avalonia.Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Show login window first; on success open the main window
            var loginWindow    = _serviceProvider.GetRequiredService<LoginWindow>();
            // LoginViewModel is Transient — retrieve the exact instance the window was given
            var loginViewModel = (LoginViewModel)loginWindow.DataContext!;

            loginViewModel.LoginSucceeded += () =>
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                desktop.MainWindow = mainWindow; // Update MainWindow BEFORE closing login
                mainWindow.Show();
                loginWindow.Close();
            };

            desktop.MainWindow = loginWindow;
            desktop.Exit += (_, _) => _serviceProvider.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        // gRPC channel — single shared instance, address matches the API's HTTPS profile
        services.AddSingleton<GrpcChannelManager>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GrpcChannelManager>>();
            return new GrpcChannelManager("https://localhost:7003", logger);
        });

        // Session context
        services.AddSingleton<SessionContext>();

        // gRPC API services
        services.AddScoped<GrpcLoanApiService>();
        services.AddScoped<GrpcRetailAccountApiService>();
        services.AddScoped<GrpcBankClientApiService>();
        services.AddScoped<GrpcUserApiService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<LoansViewModel>();
        services.AddSingleton<RetailAccountsViewModel>();
        services.AddSingleton<BankClientsViewModel>();
        services.AddSingleton<UsersViewModel>();
        services.AddTransient<LoginViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
        services.AddTransient<LoginWindow>();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });
    }
}
