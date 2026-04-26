using BankerDeskOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Configurations
{
    /// <summary>
    /// Entity configuration for the Entry entity using Fluent API.
    /// </summary>
    public class EntryConfiguration : IEntityTypeConfiguration<Entry>
    {
        public void Configure(EntityTypeBuilder<Entry> builder)
        {
            // Configure table name
            builder.ToTable("Entries");

            // Configure primary key
            builder.HasKey(x => x.Id);

            // Configure properties
            builder.Property(x => x.Id)
                .HasColumnType("uniqueidentifier")
                .ValueGeneratedNever();

            builder.Property(x => x.TransactionId)
                .IsRequired()
                .HasColumnType("uniqueidentifier");

            builder.Property(x => x.AccountId)
                .IsRequired()
                .HasColumnType("uniqueidentifier");

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.EntryType)
                .IsRequired()
                .HasColumnType("int")
                .HasConversion<int>();

            builder.Property(x => x.BalanceAfter)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.Description)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure indexes
            builder.HasIndex(x => x.TransactionId)
                .HasDatabaseName("IX_Entries_TransactionId");

            builder.HasIndex(x => x.AccountId)
                .HasDatabaseName("IX_Entries_AccountId");

            // Configure foreign key to Transaction
            builder.HasOne(e => e.Transaction)
                .WithMany(t => t.Entries)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure foreign key to RetailAccount
            builder.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}