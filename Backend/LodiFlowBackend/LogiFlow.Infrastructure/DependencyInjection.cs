using LogiFlow.Application.Abstractions;
using LogiFlow.Application.Abstractions.Repositories;
using LogiFlow.Infrastructure.Persistence;
using LogiFlow.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogiFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ICargoRepository, EfCargoRepository>();
        services.AddScoped<IVehicleRepository, EfVehicleRepository>();
        services.AddScoped<IDriverRepository, EfDriverRepository>();
        services.AddScoped<ITripRepository, EfTripRepository>();

        return services;
    }
}
