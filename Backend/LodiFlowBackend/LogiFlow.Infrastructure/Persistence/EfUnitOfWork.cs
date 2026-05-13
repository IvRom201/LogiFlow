using LogiFlow.Application.Abstractions;

namespace LogiFlow.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return new EfAppTransaction(transaction);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
