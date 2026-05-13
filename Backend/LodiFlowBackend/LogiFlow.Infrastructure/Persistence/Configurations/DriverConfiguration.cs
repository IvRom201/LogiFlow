using LogiFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiFlow.Infrastructure.Persistence.Configurations;

internal sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("drivers");

        builder.HasKey(driver => driver.Id);

        builder.Property(driver => driver.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(driver => driver.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(driver => driver.LicenseNumber)
            .HasColumnName("license_number")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(driver => driver.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(driver => driver.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(driver => driver.LicenseNumber).IsUnique();
        builder.HasIndex(driver => driver.Status);
    }
}
