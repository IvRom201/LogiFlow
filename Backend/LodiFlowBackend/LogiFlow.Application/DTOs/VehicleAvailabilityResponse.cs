namespace LogiFlow.Application.DTOs;

public sealed record VehicleAvailabilityResponse
{
    public Guid VehicleId { get; init; }
    public bool Available { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
}
