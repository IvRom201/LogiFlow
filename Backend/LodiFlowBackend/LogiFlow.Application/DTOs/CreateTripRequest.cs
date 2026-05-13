namespace LogiFlow.Application.DTOs;

public sealed record CreateTripRequest
{
    public Guid CargoId { get; init; }
    public Guid VehicleId { get; init; }
    public Guid DriverId { get; init; }
    public string Origin { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
    public DateTimeOffset ScheduledStart { get; init; }
    public DateTimeOffset ScheduledEnd { get; init; }
}
