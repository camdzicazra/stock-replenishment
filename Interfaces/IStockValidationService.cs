using StockReplenishment.Models;

namespace StockReplenishment.Interfaces;

public interface IStockValidationService
{
    Task<bool> ValidateStockAvailabilityAsync(ReplenishmentRequest request);
}