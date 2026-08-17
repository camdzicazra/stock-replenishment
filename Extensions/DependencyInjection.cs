using Microsoft.EntityFrameworkCore;
using StockReplenishment.Data;
using StockReplenishment.Interfaces;
using StockReplenishment.Services;

namespace StockReplenishment.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("StockReplenishmentDb"));

        services.AddScoped<IStockValidationService, StockValidationService>();
        services.AddScoped<IReplenishmentRequestService, ReplenishmentRequestService>();

        return services;
    }
}