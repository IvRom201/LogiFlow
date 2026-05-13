using LogiFlow.Application.DTOs;
using MediatR;

namespace LogiFlow.Application.Vehicles.Queries;

public sealed record CheckVehicleAvailabilityQuery(Guid VehicleId) : IRequest<VehicleAvailabilityResponse>;
