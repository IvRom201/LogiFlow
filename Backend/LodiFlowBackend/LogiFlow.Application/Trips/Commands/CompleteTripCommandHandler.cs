using LogiFlow.Application.Abstractions;
using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Application.Common.Exceptions;
using LogiFlow.Application.DTOs;
using LogiFlow.Application.Trips.Mapping;
using LogiFlow.Domain.Common;
using LogiFlow.Domain.Entities;
using MediatR;

namespace LogiFlow.Application.Trips.Commands;

public sealed class CompleteTripCommandHandler(
    ITripRepository tripRepository,
    ICargoRepository cargoRepository,
    IVehicleRepository vehicleRepository,
    IDriverRepository driverRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteTripCommand, TripResponse>
{
    public async Task<TripResponse> Handle(CompleteTripCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var trip = await tripRepository.GetByIdForUpdateAsync(command.TripId, cancellationToken)
                       ?? throw new NotFoundException($"Trip '{command.TripId}' was not found.");

            var cargo = await cargoRepository.GetByIdForUpdateAsync(trip.CargoId, cancellationToken)
                        ?? throw new NotFoundException($"Cargo '{trip.CargoId}' was not found.");

            var vehicle = await vehicleRepository.GetByIdForUpdateAsync(trip.VehicleId, cancellationToken)
                          ?? throw new NotFoundException($"Vehicle '{trip.VehicleId}' was not found.");

            var driver = await driverRepository.GetByIdForUpdateAsync(trip.DriverId, cancellationToken)
                         ?? throw new NotFoundException($"Driver '{trip.DriverId}' was not found.");

            trip.Complete();
            cargo.MarkDelivered();
            vehicle.Release();
            driver.Release();

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
