using LogiFlow.Domain.Common;
using LogiFlow.Domain.Enums;

namespace LogiFlow.Domain.Entities;

public sealed class Vehicle : Entity
{
    private Vehicle() { }

    private Vehicle(Guid id, string plateNumber, string make, string model, decimal maxWeightKg)
    {
        Id = id;
        PlateNumber = plateNumber;
        Make = make;
        Model = model;
        MaxWeightKg = maxWeightKg;
        Status = VehicleStatus.Idle;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string PlateNumber { get; private set; } = string.Empty;
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public decimal MaxWeightKg { get; private set; }
    public VehicleStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Vehicle Create(Guid id, string plateNumber, string make, string model, decimal maxWeightKg)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Vehicle id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(plateNumber))
        {
            throw new DomainRuleException("Vehicle plate number is required.");
        }

        if (string.IsNullOrWhiteSpace(make))
        {
            throw new DomainRuleException("Vehicle make is required.");
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new DomainRuleException("Vehicle model is required.");
        }

        if (maxWeightKg <= 0)
        {
            throw new DomainRuleException("Vehicle max weight must be greater than zero.");
        }

        return new Vehicle(id, plateNumber.Trim().ToUpperInvariant(), make.Trim(), model.Trim(), maxWeightKg);
    }

    public bool IsAvailableFor(decimal cargoWeightKg) => Status == VehicleStatus.Idle && cargoWeightKg <= MaxWeightKg;

    public void AssignToTrip(decimal cargoWeightKg)
    {
        if (Status != VehicleStatus.Idle)
        {
            throw new DomainRuleException($"Vehicle is not available. Current status: {Status}.");
        }

        if (cargoWeightKg > MaxWeightKg)
        {
            throw new DomainRuleException($"Vehicle capacity {MaxWeightKg} kg is lower than cargo weight {cargoWeightKg} kg.");
        }

        Status = VehicleStatus.Busy;
    }

    public void Release()
    {
        if (Status != VehicleStatus.Busy)
        {
            throw new DomainRuleException("Only a busy vehicle can be released.");
        }

        Status = VehicleStatus.Idle;
    }

    public void SendToMaintenance()
    {
        if (Status == VehicleStatus.Busy)
        {
            throw new DomainRuleException("A busy vehicle cannot be moved to maintenance.");
        }

        Status = VehicleStatus.Maintenance;
    }

    public void ReturnFromMaintenance()
    {
        if (Status != VehicleStatus.Maintenance)
        {
            throw new DomainRuleException("Only a vehicle in maintenance can return to idle.");
        }

        Status = VehicleStatus.Idle;
    }
}
