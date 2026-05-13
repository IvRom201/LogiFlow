using LogiFlow.Domain.Entities;

namespace LogiFlow.Application.Abstractions.Repositories;

public interface ITripRepository
{
    Task AddAsync(Trip trip, CancellationToken cancellationToken);
    Task<IReadOnlyList<Trip>> GetActiveAsync(string? search, CancellationToken cancellationToken);
}
