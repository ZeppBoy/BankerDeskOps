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
        /// DbSet for User entities.
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// DbSet for Transaction entities.
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; } = null!;

        /// <summary>
        /// DbSet for Entry entities (double-entry bookkeeping).
        /// </summary>
        public DbSet<Entry> Entries { get; set; } = null!;

        /// <summary>
        /// DbSet for Currency entities.
        /// </summary>
        public DbSet<Currency> Currencies { get; set; } = null!;

        /// <summary>
        /// DbSet for Product entities.
        /// </summary>
        public DbSet<Product> Products { get; set; } = null!;

        /// <summary>
        /// DbSet for Rate entities.
        /// </summary>
        public DbSet<Rate> Rates { get; set; } = null!;

        /// <summary>
        /// DbSet for Fee entities.
        /// </summary>
        public DbSet<Fee> Fees { get; set; } = null!;

        /// <summary>
        /// DbSet for Commission entities.
        /// </summary>
        public DbSet<Commission> Commissions { get; set; } = null!;

        /// <summary>
        /// DbSet for LoanApplication entities.
        /// </summary>
        public DbSet<LoanApplication> LoanApplications { get; set; } = null!;

        /// <summary>
        /// DbSet for RepaymentSchedule entities.
        /// </summary>
        public DbSet<RepaymentSchedule> RepaymentSchedules { get; set; } = null!;

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
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionConfiguration());
            modelBuilder.ApplyConfiguration(new EntryConfiguration());
            modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new RateConfiguration());
            modelBuilder.ApplyConfiguration(new FeeConfiguration());
            modelBuilder.ApplyConfiguration(new CommissionConfiguration());
            modelBuilder.ApplyConfiguration(new LoanApplicationConfiguration());
            modelBuilder.ApplyConfiguration(new RepaymentScheduleConfiguration());
        }
    }
}
