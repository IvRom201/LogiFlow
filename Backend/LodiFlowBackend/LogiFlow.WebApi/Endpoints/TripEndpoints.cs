using LogiFlow.Application.DTOs;
using LogiFlow.Application.Trips.Commands;
using LogiFlow.Application.Trips.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LogiFlow.WebApi.Endpoints;

public static class TripEndpoints
{
    public static IEndpointRouteBuilder MapTripEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/trips")
            .WithTags("Trips")
            .RequireAuthorization();

        group.MapGet("/active", async (
            [FromQuery] string? search,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetActiveTripsQuery(search), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetActiveTrips")
        .Produces<IReadOnlyList<TripResponse>>(StatusCodes.Status200OK);

        group.MapPost("/", async (
            [FromBody] CreateTripRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CreateTripCommand(request), cancellationToken);
            return Results.Created($"/api/trips/{result.Id}", result);
        })
        .WithName("CreateTrip")
        .Produces<TripResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}/complete", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CompleteTripCommand(id), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CompleteTrip")
        .Produces<TripResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}/cancel", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CancelTripCommand(id), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CancelTrip")
        .Produces<TripResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}