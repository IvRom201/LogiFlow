using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiFlow.Infrastructure.Persistence.Repositories;

internal sealed class EfCargoRepository(AppDbContext context) : ICargoRepository
{
    public Task<Cargo?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Cargoes
            .FromSqlInterpolated($"SELECT * FROM cargoes WHERE id = {id} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
    }
}
