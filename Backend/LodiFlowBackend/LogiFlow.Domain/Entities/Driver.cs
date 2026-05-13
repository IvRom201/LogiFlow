using LogiFlow.Domain.Common;
using LogiFlow.Domain.Enums;

namespace LogiFlow.Domain.Entities;

public sealed class Driver : Entity
{
    private Driver() { }

    private Driver(Guid id, string fullName, string licenseNumber)
    {
        Id = id;
        FullName = fullName;
        LicenseNumber = licenseNumber;
        Status = DriverStatus.Available;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string FullName { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public DriverStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Driver Create(Guid id, string fullName, string licenseNumber)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Driver id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainRuleException("Driver full name is required.");
        }

        if (string.IsNullOrWhiteSpace(licenseNumber))
        {
            throw new DomainRuleException("Driver license number is required.");
        }

        return new Driver(id, fullName.Trim(), licenseNumber.Trim().ToUpperInvariant());
    }

    public bool IsAvailable => Status == DriverStatus.Available;

    public void AssignToTrip()
    {
        if (!IsAvailable)
        {
            throw new DomainRuleException($"Driver is not available. Current status: {Status}.");
        }

        Status = DriverStatus.OnTrip;
    }

    public void Release()
    {
        if (Status != DriverStatus.OnTrip)
        {
            throw new DomainRuleException("Only an assigned driver can be released.");
        }

        Status = DriverStatus.Available;
    }

    public void MarkOffDuty()
    {
        if (Status == DriverStatus.OnTrip)
        {
            throw new DomainRuleException("A driver on trip cannot be marked off duty.");
        }

        Status = DriverStatus.OffDuty;
    }
}
