using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Application.Common.Exceptions;
using LogiFlow.Application.DTOs;
using LogiFlow.Domain.Enums;
using MediatR;

namespace LogiFlow.Application.Vehicles.Queries;

public sealed class CheckVehicleAvailabilityQueryHandler(IVehicleRepository vehicleRepository)
    : IRequestHandler<CheckVehicleAvailabilityQuery, VehicleAvailabilityResponse>
{
    public async Task<VehicleAvailabilityResponse> Handle(CheckVehicleAvailabilityQuery request, CancellationToken cancellationToken)
    {
        if (request.VehicleId == Guid.Empty)
        {
            throw new BadRequestException("Vehicle id is required.");
        }

        var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new NotFoundException($"Vehicle '{request.VehicleId}' was not found.");

        var available = vehicle.Status == VehicleStatus.Idle;

        return new VehicleAvailabilityResponse
        {
            VehicleId = vehicle.Id,
            Available = available,
            Status = vehicle.Status.ToString(),
            Reason = available ? null : $"Vehicle is {vehicle.Status}."
        };
    }
}
