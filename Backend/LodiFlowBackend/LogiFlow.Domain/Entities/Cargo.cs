using LogiFlow.Domain.Common;
using LogiFlow.Domain.Enums;

namespace LogiFlow.Domain.Entities;

public sealed class Cargo : Entity
{
    private Cargo() { }

    private Cargo(Guid id, string description, decimal weightKg)
    {
        Id = id;
        Description = description;
        WeightKg = weightKg;
        Status = CargoStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Description { get; private set; } = string.Empty;
    public decimal WeightKg { get; private set; }
    public CargoStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Cargo Create(Guid id, string description, decimal weightKg)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Cargo id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainRuleException("Cargo description is required.");
        }

        if (weightKg <= 0)
        {
            throw new DomainRuleException("Cargo weight must be greater than zero.");
        }

        return new Cargo(id, description.Trim(), weightKg);
    }

    public bool CanBeAssigned => Status == CargoStatus.Pending;

    public void AssignToTrip()
    {
        if (!CanBeAssigned)
        {
            throw new DomainRuleException($"Cargo cannot be assigned while status is {Status}.");
        }

        Status = CargoStatus.Assigned;
    }

    public void MarkInTransit()
    {
        if (Status != CargoStatus.Assigned)
        {
            throw new DomainRuleException("Cargo can be moved to transit only after assignment.");
        }

        Status = CargoStatus.InTransit;
    }

    public void MarkDelivered()
    {
        if (Status is not CargoStatus.Assigned and not CargoStatus.InTransit)
        {
            throw new DomainRuleException("Cargo can be delivered only after assignment or transit.");
        }

        Status = CargoStatus.Delivered;
    }
}
