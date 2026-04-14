using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for BankerDeskOps database.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the AppDbContext class.
        /// </summary>
        /// <param name="options">The context options.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet for Loan entities.
        /// </summary>
        public DbSet<Loan> Loans { get; set; } = null!;

        /// <summary>
        /// DbSet for RetailAccount entities.
        /// </summary>
        public DbSet<RetailAccount> RetailAccounts { get; set; } = null!;

        /// <summary>
        /// DbSet for BankClient entities.
        /// </summary>
        public DbSet<BankClient> BankClients { get; set; } = null!;

        /// <summary>
        /// DbSet for Contract entities. Created automatically on loan disbursement.
        /// </summary>
        public DbSet<Contract> Contracts { get; set; } = null!;

        /// <summary>
        /// Configures the model by applying entity configurations.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new LoanConfiguration());
            modelBuilder.ApplyConfiguration(new RetailAccountConfiguration());
            modelBuilder.ApplyConfiguration(new BankClientConfiguration());
            modelBuilder.ApplyConfiguration(new ContractConfiguration());
        }
    }
}
