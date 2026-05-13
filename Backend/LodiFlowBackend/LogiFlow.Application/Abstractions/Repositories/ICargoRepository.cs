using LogiFlow.Domain.Entities;

namespace LogiFlow.Application.Abstractions.Repositories;

public interface ICargoRepository
{
    Task<Cargo?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
}
