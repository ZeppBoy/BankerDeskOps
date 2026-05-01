using BankerDeskOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Configurations
{
    public class RateConfiguration : IEntityTypeConfiguration<Rate>
    {
        public void Configure(EntityTypeBuilder<Rate> builder)
        {
            builder.ToTable("Rates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnType("uniqueidentifier")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.SinceDate)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(x => x.ToDate)
                .HasColumnType("datetime2");

            builder.Property(x => x.RateType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.RateValue)
                .IsRequired()
                .HasColumnType("decimal(5, 4)");

            builder.HasIndex(x => new { x.SinceDate, x.ToDate })
                .HasDatabaseName("IX_Rates_DateRange");
        }
    }
}
