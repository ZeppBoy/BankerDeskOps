using BankerDeskOps.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankerDeskOps.Api.Tests
{
    /// <summary>
    /// Custom web application factory for testing with in-memory SQLite database.
    /// </summary>
    public class ApiTestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the SQL Server DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Create a new in-memory SQLite connection for each factory
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                // Add DbContext with in-memory SQLite
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                // Build the service provider
                var serviceProvider = services.BuildServiceProvider();

                // Create the database schema
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    // Drop existing schema to avoid "table already exists" errors
                    dbContext.Database.EnsureDeleted();
                    // Create fresh schema
                    dbContext.Database.EnsureCreated();
                }
            });

            builder.ConfigureAppConfiguration((context, conf) =>
            {
                // Override connection string to prevent migrations from running
                // We handle schema creation manually above
            });
        }

        public override async ValueTask DisposeAsync()
        {
            _connection?.Dispose();
            await base.DisposeAsync();
        }
    }
}
