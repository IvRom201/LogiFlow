using LogiFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiFlow.Infrastructure.Persistence.Configurations;

internal sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("trips");

        builder.HasKey(trip => trip.Id);

        builder.Property(trip => trip.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(trip => trip.CargoId).HasColumnName("cargo_id").IsRequired();
        builder.Property(trip => trip.VehicleId).HasColumnName("vehicle_id").IsRequired();
        builder.Property(trip => trip.DriverId).HasColumnName("driver_id").IsRequired();

        builder.Property(trip => trip.Origin)
            .HasColumnName("origin")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(trip => trip.Destination)
            .HasColumnName("destination")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(trip => trip.ScheduledStart)
            .HasColumnName("scheduled_start")
            .IsRequired();

        builder.Property(trip => trip.ScheduledEnd)
            .HasColumnName("scheduled_end")
            .IsRequired();

        builder.Property(trip => trip.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(trip => trip.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(trip => trip.CompletedAt)
            .HasColumnName("completed_at");

        builder.HasOne(trip => trip.Cargo)
            .WithMany()
            .HasForeignKey(trip => trip.CargoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(trip => trip.Vehicle)
            .WithMany()
            .HasForeignKey(trip => trip.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(trip => trip.Driver)
            .WithMany()
            .HasForeignKey(trip => trip.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(trip => trip.Status);
        builder.HasIndex(trip => new { trip.VehicleId, trip.Status });
        builder.HasIndex(trip => new { trip.DriverId, trip.Status });
    }
}
