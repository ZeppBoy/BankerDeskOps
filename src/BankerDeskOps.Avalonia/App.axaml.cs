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
            desktop.MainWindow = _serviceProvider.GetRequiredService<MainWindow>();

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

        // gRPC API services
        services.AddScoped<GrpcLoanApiService>();
        services.AddScoped<GrpcRetailAccountApiService>();
        services.AddScoped<GrpcBankClientApiService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<LoansViewModel>();
        services.AddSingleton<RetailAccountsViewModel>();
        services.AddSingleton<BankClientsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });
    }
}
