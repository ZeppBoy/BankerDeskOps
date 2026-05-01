using BankerDeskOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnType("uniqueidentifier")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Term)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(x => x.MinAmount)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.MaxAmount)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.Description)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            builder.Property(x => x.Fees)
                .HasMaxLength(1000)
                .HasColumnType("nvarchar(1000)");

            builder.Property(x => x.Commissions)
                .HasMaxLength(1000)
                .HasColumnType("nvarchar(1000)");

            builder.HasOne(p => p.Currency)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Term)
                .HasDatabaseName("IX_Products_Term");
        }
    }
}
