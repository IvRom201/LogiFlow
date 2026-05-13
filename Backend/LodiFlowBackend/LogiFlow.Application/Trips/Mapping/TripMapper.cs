using LogiFlow.Application.DTOs;
using LogiFlow.Domain.Entities;

namespace LogiFlow.Application.Trips.Mapping;

internal static class TripMapper
{
    internal static TripResponse ToResponse(Trip trip) => new()
    {
        Id = trip.Id,
        CargoId = trip.CargoId,
        CargoDescription = trip.Cargo.Description,
        CargoWeightKg = trip.Cargo.WeightKg,
        VehicleId = trip.VehicleId,
        VehiclePlateNumber = trip.Vehicle.PlateNumber,
        DriverId = trip.DriverId,
        DriverFullName = trip.Driver.FullName,
        Origin = trip.Origin,
        Destination = trip.Destination,
        ScheduledStart = trip.ScheduledStart,
        ScheduledEnd = trip.ScheduledEnd,
        Status = trip.Status.ToString()
    };
}
