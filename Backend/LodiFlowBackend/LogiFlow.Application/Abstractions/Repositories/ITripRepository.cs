using LogiFlow.Domain.Entities;

namespace LogiFlow.Application.Abstractions.Repositories;

public interface ITripRepository
{
    Task AddAsync(Trip trip, CancellationToken cancellationToken);

    Task<Trip?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Trip>> GetActiveAsync(string? search, CancellationToken cancellationToken);
}
