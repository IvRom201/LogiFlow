namespace LogiFlow.Application.DTOs;

public sealed record TripResponse
{
    public Guid Id { get; init; }
    public Guid CargoId { get; init; }
    public string CargoDescription { get; init; } = string.Empty;
    public decimal CargoWeightKg { get; init; }
    public Guid VehicleId { get; init; }
    public string VehiclePlateNumber { get; init; } = string.Empty;
    public Guid DriverId { get; init; }
    public string DriverFullName { get; init; } = string.Empty;
    public string Origin { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
    public DateTimeOffset ScheduledStart { get; init; }
    public DateTimeOffset ScheduledEnd { get; init; }
    public string Status { get; init; } = string.Empty;
}
