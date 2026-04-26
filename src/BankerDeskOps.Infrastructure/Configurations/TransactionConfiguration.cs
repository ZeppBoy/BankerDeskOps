using BankerDeskOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Configurations
{
    /// <summary>
    /// Entity configuration for the Transaction entity using Fluent API.
    /// </summary>
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            // Configure table name
            builder.ToTable("Transactions");

            // Configure primary key
            builder.HasKey(x => x.Id);

            // Configure properties
            builder.Property(x => x.Id)
                .HasColumnType("uniqueidentifier")
                .ValueGeneratedNever();

            builder.Property(x => x.TransactionType)
                .IsRequired()
                .HasColumnType("int")
                .HasConversion<int>();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasColumnType("int")
                .HasConversion<int>()
                .HasDefaultValue(Domain.Enums.TransactionStatus.Pending);

            builder.Property(x => x.ReferenceId)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure indexes
            builder.HasIndex(x => x.ReferenceId)
                .IsUnique()
                .HasDatabaseName("UX_Transactions_ReferenceId");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_Transactions_Status");

            builder.HasIndex(x => x.CreatedAt)
                .HasDatabaseName("IX_Transactions_CreatedAt");

            // Configure relationship with Entries
            builder.HasMany(t => t.Entries)
                .WithOne(e => e.Transaction)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}