using System.Configuration;
using System.Data;
using System.Windows;
using BankerDeskOps.Wpf.Services;
using BankerDeskOps.Wpf.ViewModels;
using BankerDeskOps.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;

namespace BankerDeskOps.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Create and show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Add gRPC channel manager
            services.AddSingleton<GrpcChannelManager>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<GrpcChannelManager>>();
                return new GrpcChannelManager("https://localhost:7074", logger); // Update with your gRPC API URL
            });

            // Add gRPC API services
            services.AddScoped<GrpcLoanApiService>();
            services.AddScoped<GrpcRetailAccountApiService>();

            // Keep HTTP client and services for backward compatibility (optional)
            services.AddHttpClient<ApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7074/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddScoped<LoanApiService>();
            services.AddScoped<RetailAccountApiService>();

            // Add ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<LoansViewModel>();
            services.AddSingleton<RetailAccountsViewModel>();

            // Add Views
            services.AddSingleton<LoansView>();
            services.AddSingleton<RetailAccountsView>();
            services.AddSingleton<MainWindow>();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });
        }
    }
}

