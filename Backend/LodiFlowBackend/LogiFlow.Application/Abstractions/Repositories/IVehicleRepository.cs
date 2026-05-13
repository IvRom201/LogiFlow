using LogiFlow.Domain.Entities;

namespace LogiFlow.Application.Abstractions.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Vehicle?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
}
