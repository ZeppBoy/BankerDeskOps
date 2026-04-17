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

            // Show login window first
            var loginView = _serviceProvider.GetRequiredService<LoginView>();
            var loginViewModel = (LoginViewModel)loginView.DataContext;

            loginViewModel.LoginSucceeded += () =>
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                loginView.Close();
            };

            loginView.Show();
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
                return new GrpcChannelManager("https://localhost:7003", logger); // Match the API's https profile port
            });

            // Add session context
            services.AddSingleton<SessionContext>();

            // Add gRPC API services
            services.AddScoped<GrpcLoanApiService>();
            services.AddScoped<GrpcRetailAccountApiService>();
            services.AddScoped<GrpcBankClientApiService>();
            services.AddScoped<GrpcUserApiService>();

            // Add ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<LoansViewModel>();
            services.AddSingleton<RetailAccountsViewModel>();
            services.AddSingleton<BankClientsViewModel>();
            services.AddSingleton<UsersViewModel>();
            services.AddTransient<LoginViewModel>();

            // Add Views
            services.AddSingleton<LoansView>();
            services.AddSingleton<RetailAccountsView>();
            services.AddSingleton<BankClientsView>();
            services.AddSingleton<UsersView>();
            services.AddTransient<LoginView>();
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

