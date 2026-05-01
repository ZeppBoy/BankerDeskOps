using BankerDeskOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Configurations
{
    public class RepaymentScheduleConfiguration : IEntityTypeConfiguration<RepaymentSchedule>
    {
        public void Configure(EntityTypeBuilder<RepaymentSchedule> builder)
        {
            builder.ToTable("RepaymentSchedules");

            builder.HasKey(x => x.ScheduleId);

            builder.Property(x => x.ScheduleId)
                .HasColumnType("uniqueidentifier")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.PlannedDate)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(x => x.Capital)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.Interest)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.Saldo)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.EIR)
                .IsRequired()
                .HasColumnType("decimal(5, 4)");

            builder.HasOne(rs => rs.LoanApplication)
                .WithMany(la => la.RepaymentSchedules)
                .HasForeignKey(rs => rs.LoanApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.LoanApplicationId)
                .HasDatabaseName("IX_RepaymentSchedules_LoanApplicationId");

            builder.HasIndex(x => new { x.LoanApplicationId, x.PlannedDate })
                .HasDatabaseName("IX_RepaymentSchedules_LoanApplication_PlannedDate");
        }
    }
}
