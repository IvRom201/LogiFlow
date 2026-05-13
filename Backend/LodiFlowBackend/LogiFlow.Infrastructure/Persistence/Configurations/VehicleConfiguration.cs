using LogiFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiFlow.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(vehicle => vehicle.Id);

        builder.Property(vehicle => vehicle.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(vehicle => vehicle.PlateNumber)
            .HasColumnName("plate_number")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(vehicle => vehicle.Make)
            .HasColumnName("make")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(vehicle => vehicle.Model)
            .HasColumnName("model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(vehicle => vehicle.MaxWeightKg)
            .HasColumnName("max_weight_kg")
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(vehicle => vehicle.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(vehicle => vehicle.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(vehicle => vehicle.PlateNumber).IsUnique();
        builder.HasIndex(vehicle => vehicle.Status);
    }
}
