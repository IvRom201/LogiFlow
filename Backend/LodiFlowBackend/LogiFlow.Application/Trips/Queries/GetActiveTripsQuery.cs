using LogiFlow.Application.DTOs;
using MediatR;

namespace LogiFlow.Application.Trips.Queries;

public sealed record GetActiveTripsQuery(string? Search) : IRequest<IReadOnlyList<TripResponse>>;
