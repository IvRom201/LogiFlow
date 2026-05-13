using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Application.DTOs;
using LogiFlow.Application.Trips.Mapping;
using MediatR;

namespace LogiFlow.Application.Trips.Queries;

public sealed class GetActiveTripsQueryHandler(ITripRepository tripRepository)
    : IRequestHandler<GetActiveTripsQuery, IReadOnlyList<TripResponse>>
{
    public async Task<IReadOnlyList<TripResponse>> Handle(GetActiveTripsQuery request, CancellationToken cancellationToken)
    {
        var trips = await tripRepository.GetActiveAsync(request.Search, cancellationToken);
        return trips.Select(TripMapper.ToResponse).ToList();
    }
}
