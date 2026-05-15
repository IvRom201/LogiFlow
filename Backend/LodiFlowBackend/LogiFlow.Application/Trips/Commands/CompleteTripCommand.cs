using LogiFlow.Application.DTOs;
using MediatR;

namespace LogiFlow.Application.Trips.Commands;

public sealed record CompleteTripCommand(Guid TripId) : IRequest<TripResponse>;