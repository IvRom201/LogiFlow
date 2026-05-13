using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Domain.Entities;
using LogiFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogiFlow.Infrastructure.Persistence.Repositories;

internal sealed class EfTripRepository(AppDbContext context) : ITripRepository
{
    public async Task AddAsync(Trip trip, CancellationToken cancellationToken)
    {
        await context.Trips.AddAsync(trip, cancellationToken);
    }

    public async Task<IReadOnlyList<Trip>> GetActiveAsync(string? search, CancellationToken cancellationToken)
    {
        var query = context.Trips
            .AsNoTracking()
            .Include(trip => trip.Cargo)
            .Include(trip => trip.Vehicle)
            .Include(trip => trip.Driver)
            .Where(trip => trip.Status == TripStatus.Active || trip.Status == TripStatus.Planned);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(trip =>
                EF.Functions.ILike(trip.Origin, pattern) ||
                EF.Functions.ILike(trip.Destination, pattern) ||
                EF.Functions.ILike(trip.Vehicle.PlateNumber, pattern) ||
                EF.Functions.ILike(trip.Driver.FullName, pattern) ||
                EF.Functions.ILike(trip.Cargo.Description, pattern));
        }

        return await query
            .OrderBy(trip => trip.ScheduledStart)
            .ToListAsync(cancellationToken);
    }
}
