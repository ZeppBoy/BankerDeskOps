using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankerDeskOps.Infrastructure
{
    /// <summary>
    /// Design-time DbContext factory for Entity Framework Core migrations.
    /// This factory is used by EF Core tools when creating/updating migrations.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Default development connection string
            // In production, this would be configured differently
            const string connectionString = "Server=.;Database=BankerDeskOps;Trusted_Connection=true;TrustServerCertificate=true";

            optionsBuilder.UseSqlServer(connectionString,
                sqlServerOptions => sqlServerOptions.MigrationsAssembly(GetType().Assembly.GetName().Name));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
