using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Infrastructure.Data;
using BankerDeskOps.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankerDeskOps.Infrastructure
{
    /// <summary>
    /// Dependency injection extension methods for infrastructure services.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds infrastructure services including DbContext and repositories to the DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionString">The SQL Server connection string.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            string connectionString)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            // Register DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString,
                    sqlServerOptions => sqlServerOptions.MigrationsAssembly(typeof(DependencyInjection).Assembly.GetName().Name)));

            // Register repositories
            services.AddScoped<ILoanRepository, LoanRepository>();
            services.AddScoped<IRetailAccountRepository, RetailAccountRepository>();

            return services;
        }
    }
}
