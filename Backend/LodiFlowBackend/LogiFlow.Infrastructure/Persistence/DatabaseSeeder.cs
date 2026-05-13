using LogiFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiFlow.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedVehiclesAsync(context, cancellationToken);
        await SeedDriversAsync(context, cancellationToken);
        await SeedCargoesAsync(context, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedVehiclesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Vehicles.AnyAsync(cancellationToken))
        {
            return;
        }

        var first = Vehicle.Create(Guid.Parse("11111111-1111-1111-1111-111111111111"), "WA 1001L", "Volvo", "FH16", 24000);
        var second = Vehicle.Create(Guid.Parse("22222222-2222-2222-2222-222222222222"), "WA 2002L", "MAN", "TGX", 18000);
        second.SendToMaintenance();

        await context.Vehicles.AddRangeAsync([first, second], cancellationToken);
    }

    private static async Task SeedDriversAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Drivers.AnyAsync(cancellationToken))
        {
            return;
        }

        var first = Driver.Create(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Adam Nowak", "PL-DRV-1001");
        var second = Driver.Create(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Marta Kowalska", "PL-DRV-2002");
        second.MarkOffDuty();

        await context.Drivers.AddRangeAsync([first, second], cancellationToken);
    }

    private static async Task SeedCargoesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Cargoes.AnyAsync(cancellationToken))
        {
            return;
        }

        var first = Cargo.Create(Guid.Parse("99999999-9999-9999-9999-999999999999"), "Pallets with electronics", 1200);
        var second = Cargo.Create(Guid.Parse("88888888-8888-8888-8888-888888888888"), "Industrial machine parts", 4200);

        await context.Cargoes.AddRangeAsync([first, second], cancellationToken);
    }
}
