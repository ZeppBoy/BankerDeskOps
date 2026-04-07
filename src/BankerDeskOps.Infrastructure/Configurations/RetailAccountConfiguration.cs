using BankerDeskOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Configurations
{
    /// <summary>
    /// Entity configuration for the RetailAccount entity using Fluent API.
    /// </summary>
    public class RetailAccountConfiguration : IEntityTypeConfiguration<RetailAccount>
    {
        public void Configure(EntityTypeBuilder<RetailAccount> builder)
        {
            // Configure table name
            builder.ToTable("RetailAccounts");

            // Configure primary key
            builder.HasKey(x => x.Id);

            // Configure properties
            builder.Property(x => x.Id)
                .HasColumnType("uniqueidentifier")
                .ValueGeneratedNever();

            builder.Property(x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("nvarchar(255)");

            builder.Property(x => x.AccountNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            builder.Property(x => x.Balance)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.AccountType)
                .IsRequired()
                .HasColumnType("int")
                .HasConversion<int>();

            builder.Property(x => x.OpenedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure indexes
            builder.HasIndex(x => x.AccountNumber)
                .IsUnique()
                .HasDatabaseName("UX_RetailAccounts_AccountNumber");

            builder.HasIndex(x => x.CustomerName)
                .HasDatabaseName("IX_RetailAccounts_CustomerName");

            builder.HasIndex(x => x.AccountType)
                .HasDatabaseName("IX_RetailAccounts_AccountType");
        }
    }
}
