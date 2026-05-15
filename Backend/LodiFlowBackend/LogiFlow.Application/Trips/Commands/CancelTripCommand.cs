using LogiFlow.Application.DTOs;
using MediatR;

namespace LogiFlow.Application.Trips.Commands;

public sealed record CancelTripCommand(Guid TripId) : IRequest<TripResponse>;
