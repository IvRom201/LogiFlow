using LogiFlow.Domain.Entities;
using LogiFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiFlow.Infrastructure.Persistence.Configurations;

internal sealed class CargoConfiguration : IEntityTypeConfiguration<Cargo>
{
    public void Configure(EntityTypeBuilder<Cargo> builder)
    {
        builder.ToTable("cargoes");

        builder.HasKey(cargo => cargo.Id);

        builder.Property(cargo => cargo.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(cargo => cargo.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(cargo => cargo.WeightKg)
            .HasColumnName("weight_kg")
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(cargo => cargo.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(cargo => cargo.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(cargo => cargo.Status);
    }
}
