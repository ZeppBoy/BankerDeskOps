using BankerDeskOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Configurations
{
    public class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
    {
        public void Configure(EntityTypeBuilder<LoanApplication> builder)
        {
            builder.ToTable("LoanApplications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnType("uniqueidentifier")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.PendingDate)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(x => x.DisbursementDate)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(x => x.ApprovedDate)
                .HasColumnType("datetime2");

            builder.Property(x => x.RejectedDate)
                .HasColumnType("datetime2");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.RepaymentPlan)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            builder.HasOne(la => la.Product)
                .WithMany(p => p.LoanApplications)
                .HasForeignKey(la => la.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_LoanApplications_Status");

            builder.HasIndex(x => x.CustomerId)
                .HasDatabaseName("IX_LoanApplications_CustomerId");
        }
    }
}
