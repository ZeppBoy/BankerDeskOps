using BankerDeskOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Configurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.ToTable("Contracts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnType("uniqueidentifier")
                .ValueGeneratedNever();

            builder.Property(x => x.ContractNumber)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("nvarchar(30)");

            builder.Property(x => x.LoanId)
                .IsRequired()
                .HasColumnType("uniqueidentifier");

            builder.Property(x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("nvarchar(255)");

            builder.Property(x => x.LoanAmount)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.InterestRate)
                .IsRequired()
                .HasColumnType("decimal(5, 2)");

            builder.Property(x => x.TermMonths)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(x => x.DisbursedAt)
                .IsRequired()
                .HasColumnType("datetime2");

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

            // One contract number is globally unique
            builder.HasIndex(x => x.ContractNumber)
                .IsUnique()
                .HasDatabaseName("UX_Contracts_ContractNumber");

            // One loan produces exactly one contract
            builder.HasIndex(x => x.LoanId)
                .IsUnique()
                .HasDatabaseName("UX_Contracts_LoanId");

            builder.HasIndex(x => x.CustomerName)
                .HasDatabaseName("IX_Contracts_CustomerName");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_Contracts_Status");
        }
    }
}
