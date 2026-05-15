using LogiFlow.Application.Abstractions;
using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Application.Common.Exceptions;
using LogiFlow.Application.DTOs;
using LogiFlow.Application.Trips.Mapping;
using LogiFlow.Domain.Common;
using LogiFlow.Domain.Entities;
using MediatR;

namespace LogiFlow.Application.Trips.Commands;

public sealed class CreateTripCommandHandler(
    ICargoRepository cargoRepository,
    IVehicleRepository vehicleRepository,
    IDriverRepository driverRepository,
    ITripRepository tripRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTripCommand, TripResponse>
{
    public async Task<TripResponse> Handle(CreateTripCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (request.ScheduledEnd <= request.ScheduledStart)
        {
            throw new BadRequestException("ScheduledEnd must be after ScheduledStart.");
        }

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
                throw new ConflictException("Cargo cannot be assigned.");
            }

            if (!vehicle.IsAvailableFor(cargo.WeightKg))
            {
                throw new ConflictException("Vehicle is not available or does not have enough capacity.");
            }

            if (!driver.IsAvailable)
            {
                throw new ConflictException("Driver is not available.");
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

            return TripMapper.ToResponse(trip, cargo, vehicle, driver);
        }
        catch (DomainRuleException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException(ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}