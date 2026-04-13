using BankerDeskOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankerDeskOps.Infrastructure.Configurations
{
    public class BankClientConfiguration : IEntityTypeConfiguration<BankClient>
    {
        public void Configure(EntityTypeBuilder<BankClient> builder)
        {
            builder.ToTable("BankClients");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnType("uniqueidentifier")
                .ValueGeneratedNever();

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("nvarchar(255)");

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            builder.Property(x => x.DateOfBirth)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(x => x.NationalId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            builder.Property(x => x.Street)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("nvarchar(255)");

            builder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(x => x.PostalCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            builder.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

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

            builder.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName("UX_BankClients_Email");

            builder.HasIndex(x => x.NationalId)
                .IsUnique()
                .HasDatabaseName("UX_BankClients_NationalId");

            builder.HasIndex(x => x.LastName)
                .HasDatabaseName("IX_BankClients_LastName");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_BankClients_Status");
        }
    }
}
