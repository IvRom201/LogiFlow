using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiFlow.Infrastructure.Persistence.Repositories;

internal sealed class EfDriverRepository(AppDbContext context) : IDriverRepository
{
    public Task<Driver?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Drivers
            .FromSqlInterpolated($"SELECT * FROM drivers WHERE id = {id} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
    }
}
