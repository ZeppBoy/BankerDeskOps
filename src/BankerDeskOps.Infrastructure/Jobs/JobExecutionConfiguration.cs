using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Jobs
{
    public class JobExecutionConfiguration : IEntityTypeConfiguration<JobExecution>
    {
        public void Configure(EntityTypeBuilder<JobExecution> builder)
        {
            builder.ToTable("JobExecutions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.JobName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.IdempotencyKey).IsRequired().HasMaxLength(500);
            builder.HasIndex(x => x.IdempotencyKey).IsUnique();
            builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
            builder.Property(x => x.Status).HasConversion<int>();
        }
    }
}
