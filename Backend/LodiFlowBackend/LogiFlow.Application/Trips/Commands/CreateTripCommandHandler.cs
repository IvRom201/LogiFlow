using LogiFlow.Application.Abstractions;
using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Application.Common.Exceptions;
using LogiFlow.Application.DTOs;
using LogiFlow.Domain.Common;
using LogiFlow.Domain.Entities;
using MediatR;

namespace LogiFlow.Application.Trips.Commands;

public sealed class CreateTripCommandHandler(
    ICargoRepository cargoRepository,
    IVehicleRepository vehicleRepository,
    IDriverRepository driverRepository,
    ITripRepository tripRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTripCommand, TripResponse>
{
    public async Task<TripResponse> Handle(CreateTripCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        ValidateRequestShape(request);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var cargo = await cargoRepository.GetByIdForUpdateAsync(request.CargoId, cancellationToken)
                ?? throw new NotFoundException($"Cargo '{request.CargoId}' was not found.");

            var vehicle = await vehicleRepository.GetByIdForUpdateAsync(request.VehicleId, cancellationToken)
                ?? throw new NotFoundException($"Vehicle '{request.VehicleId}' was not found.");

            var driver = await driverRepository.GetByIdForUpdateAsync(request.DriverId, cancellationToken)
                ?? throw new NotFoundException($"Driver '{request.DriverId}' was not found.");

            if (!cargo.CanBeAssigned)
            {
                throw new ConflictException($"Cargo '{cargo.Id}' cannot be assigned because current status is {cargo.Status}.");
            }

            if (!vehicle.IsAvailableFor(cargo.WeightKg))
            {
                var reason = vehicle.Status.ToString() != "Idle"
                    ? $"current status is {vehicle.Status}"
                    : $"capacity {vehicle.MaxWeightKg} kg is lower than cargo weight {cargo.WeightKg} kg";

                throw new ConflictException($"Vehicle '{vehicle.Id}' is not available: {reason}.");
            }

            if (!driver.IsAvailable)
            {
                throw new ConflictException($"Driver '{driver.Id}' is not available. Current status is {driver.Status}.");
            }

            var trip = Trip.Create(
                cargo.Id,
                vehicle.Id,
                driver.Id,
                request.Origin,
                request.Destination,
                request.ScheduledStart,
                request.ScheduledEnd);

            cargo.AssignToTrip();
            vehicle.AssignToTrip(cargo.WeightKg);
            driver.AssignToTrip();

            await tripRepository.AddAsync(trip, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new TripResponse
            {
                Id = trip.Id,
                CargoId = cargo.Id,
                CargoDescription = cargo.Description,
                CargoWeightKg = cargo.WeightKg,
                VehicleId = vehicle.Id,
                VehiclePlateNumber = vehicle.PlateNumber,
                DriverId = driver.Id,
                DriverFullName = driver.FullName,
                Origin = trip.Origin,
                Destination = trip.Destination,
                ScheduledStart = trip.ScheduledStart,
                ScheduledEnd = trip.ScheduledEnd,
                Status = trip.Status.ToString()
            };
        }
        catch (DomainRuleException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new BadRequestException(ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static void ValidateRequestShape(CreateTripRequest request)
    {
        if (request.ScheduledStart == default)
        {
            throw new BadRequestException("ScheduledStart is required.");
        }

        if (request.ScheduledEnd == default)
        {
            throw new BadRequestException("ScheduledEnd is required.");
        }

        if (request.ScheduledEnd <= request.ScheduledStart)
        {
            throw new BadRequestException("ScheduledEnd must be after ScheduledStart.");
        }
    }
}
