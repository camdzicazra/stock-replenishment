using StockReplenishment.Interfaces;
using StockReplenishment.Models;

namespace StockReplenishment.Services;

public class StockValidationService : IStockValidationService
{
    public async Task<bool> ValidateStockAvailabilityAsync(ReplenishmentRequest request)
    {
        var delay = Random.Shared.Next(3000, 6000);
        await Task.Delay(delay);

        bool isStockAvailable = Random.Shared.Next(1, 10) > 1;
        
        return isStockAvailable;
    }
}