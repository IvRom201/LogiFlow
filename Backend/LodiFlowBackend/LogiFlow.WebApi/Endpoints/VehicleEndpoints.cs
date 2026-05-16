using LogiFlow.Application.DTOs;
using LogiFlow.Application.Vehicles.Queries;
using MediatR;

namespace LogiFlow.WebApi.Endpoints;

public static class VehicleEndpoints
{
    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vehicles")
            .WithTags("Vehicles")
            .RequireAuthorization();

        group.MapGet("/{id:guid}/availability", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CheckVehicleAvailabilityQuery(id), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CheckVehicleAvailability")
        .Produces<VehicleAvailabilityResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
