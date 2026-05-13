using LogiFlow.Domain.Entities;

namespace LogiFlow.Application.Abstractions.Repositories;

public interface IDriverRepository
{
    Task<Driver?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
}
