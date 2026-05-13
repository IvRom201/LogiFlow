using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiFlow.Infrastructure.Persistence.Repositories;

internal sealed class EfVehicleRepository(AppDbContext context) : IVehicleRepository
{
    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Vehicles.AsNoTracking().SingleOrDefaultAsync(vehicle => vehicle.Id == id, cancellationToken);
    }

    public Task<Vehicle?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Vehicles
            .FromSqlInterpolated($"SELECT * FROM vehicles WHERE id = {id} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
    }
}
