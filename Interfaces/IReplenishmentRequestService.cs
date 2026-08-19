using StockReplenishment.Models;

namespace StockReplenishment.Interfaces;

public interface IReplenishmentRequestService
{
    Task<IEnumerable<ReplenishmentRequest>> GetAllRequestsAsync();
    Task<ReplenishmentRequest?> GetRequestByIdAsync(int id);
    Task<ReplenishmentRequest> CreateDraftAsync(ReplenishmentRequest request);
    Task<ReplenishmentRequest?> SubmitRequestAsync(int id);
    Task<ReplenishmentRequest?> ApproveRequestAsync(int id);
    Task<ReplenishmentRequest?> RejectRequestAsync(int id, string reason);
    Task<ReplenishmentRequest?> FulfillRequestAsync(int id, List<ItemFulfillment> fulfilledItems);
}