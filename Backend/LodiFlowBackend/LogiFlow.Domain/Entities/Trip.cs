using LogiFlow.Domain.Common;
using LogiFlow.Domain.Enums;

namespace LogiFlow.Domain.Entities;

public sealed class Trip : Entity
{
    private Trip() { }

    private Trip(
        Guid id,
        Guid cargoId,
        Guid vehicleId,
        Guid driverId,
        string origin,
        string destination,
        DateTimeOffset scheduledStart,
        DateTimeOffset scheduledEnd)
    {
        Id = id;
        CargoId = cargoId;
        VehicleId = vehicleId;
        DriverId = driverId;
        Origin = origin;
        Destination = destination;
        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
        Status = TripStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid CargoId { get; private set; }
    public Cargo Cargo { get; private set; } = null!;

    public Guid VehicleId { get; private set; }
    public Vehicle Vehicle { get; private set; } = null!;

    public Guid DriverId { get; private set; }
    public Driver Driver { get; private set; } = null!;

    public string Origin { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public DateTimeOffset ScheduledStart { get; private set; }
    public DateTimeOffset ScheduledEnd { get; private set; }
    public TripStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public static Trip Create(
        Guid cargoId,
        Guid vehicleId,
        Guid driverId,
        string origin,
        string destination,
        DateTimeOffset scheduledStart,
        DateTimeOffset scheduledEnd)
    {
        if (cargoId == Guid.Empty)
        {
            throw new DomainRuleException("Cargo id is required.");
        }

        if (vehicleId == Guid.Empty)
        {
            throw new DomainRuleException("Vehicle id is required.");
        }

        if (driverId == Guid.Empty)
        {
            throw new DomainRuleException("Driver id is required.");
        }

        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new DomainRuleException("Trip origin is required.");
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new DomainRuleException("Trip destination is required.");
        }

        if (scheduledEnd <= scheduledStart)
        {
            throw new DomainRuleException("Scheduled end must be after scheduled start.");
        }

        return new Trip(Guid.NewGuid(), cargoId, vehicleId, driverId, origin.Trim(), destination.Trim(), scheduledStart, scheduledEnd);
    }

    public void Complete()
    {
        if (Status is TripStatus.Completed or TripStatus.Cancelled)
        {
            throw new DomainRuleException($"Trip cannot be completed while status is {Status}.");
        }

        Status = TripStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == TripStatus.Completed)
        {
            throw new DomainRuleException("Completed trip cannot be cancelled.");
        }

        Status = TripStatus.Cancelled;
    }
}
