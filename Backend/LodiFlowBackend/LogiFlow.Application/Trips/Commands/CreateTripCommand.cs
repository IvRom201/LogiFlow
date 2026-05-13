using LogiFlow.Application.DTOs;
using MediatR;

namespace LogiFlow.Application.Trips.Commands;

public sealed record CreateTripCommand(CreateTripRequest Request) : IRequest<TripResponse>;
