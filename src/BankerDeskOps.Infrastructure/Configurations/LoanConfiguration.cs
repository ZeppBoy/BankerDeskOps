using BankerDeskOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Configurations
{
    /// <summary>
    /// Entity configuration for the Loan entity using Fluent API.
    /// </summary>
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            // Configure table name
            builder.ToTable("Loans");

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

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.InterestRate)
                .IsRequired()
                .HasColumnType("decimal(5, 2)");

            builder.Property(x => x.TermMonths)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasColumnType("int")
                .HasConversion<int>();

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure indexes
            builder.HasIndex(x => x.CustomerName)
                .HasDatabaseName("IX_Loans_CustomerName");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_Loans_Status");
        }
    }
}
