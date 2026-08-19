using Microsoft.EntityFrameworkCore;
using StockReplenishment.Data;
using StockReplenishment.Interfaces;
using StockReplenishment.Models;

namespace StockReplenishment.Services;

public class ReplenishmentRequestService(
    AppDbContext context, 
    IServiceScopeFactory scopeFactory) : IReplenishmentRequestService
{
    public async Task<IEnumerable<ReplenishmentRequest>> GetAllRequestsAsync()
    {
        return await context.Requests.Include(r => r.Items).ToListAsync();
    }

    public async Task<ReplenishmentRequest?> GetRequestByIdAsync(int id)
    {
        return await context.Requests.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ReplenishmentRequest> CreateDraftAsync(ReplenishmentRequest request)
    {
        request.Status = RequestStatus.Draft;
        context.Requests.Add(request);
        await context.SaveChangesAsync();
        return request;
    }

    public async Task<ReplenishmentRequest?> SubmitRequestAsync(int id)
    {
        var request = await GetRequestByIdAsync(id);
        if (request == null || request.Status != RequestStatus.Draft) return null;

        request.Status = RequestStatus.Submitted;
        await context.SaveChangesAsync();

        _ = Task.Run(async () => await PerformExternalValidationAsync(id));

        return request;
    }

    private async Task PerformExternalValidationAsync(int requestId)
{
    using var scope = scopeFactory.CreateScope();
    var bgContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var validator = scope.ServiceProvider.GetRequiredService<IStockValidationService>();

    try
    {
        var request = await bgContext.Requests.FindAsync(requestId);
        if (request == null) return;

        // Simulating the slow check
        bool isValid = await validator.ValidateStockAvailabilityAsync(request);

        if (!isValid)
        {
            request.Status = RequestStatus.Rejected;
            request.RejectionReason = "External stock validation failed: Insufficient stock.";
            await bgContext.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var failedRequest = await bgContext.Requests.FindAsync(requestId);
        if (failedRequest != null)
        {
            failedRequest.Status = RequestStatus.Rejected;
            failedRequest.RejectionReason = "System Error: External validation service failed to respond.";
            await bgContext.SaveChangesAsync();
        }
        
        Console.WriteLine($"Background validation crashed for Request {requestId}: {ex.Message}");
    }
}

    public async Task<ReplenishmentRequest?> ApproveRequestAsync(int id)
    {
        var request = await GetRequestByIdAsync(id);
        if (request == null || request.Status != RequestStatus.Submitted) return null;

        request.Status = RequestStatus.Approved;
        await context.SaveChangesAsync();
        return request;
    }

    public async Task<ReplenishmentRequest?> RejectRequestAsync(int id, string reason)
    {
        var request = await GetRequestByIdAsync(id);
        if (request == null || request.Status != RequestStatus.Submitted) return null;

        request.Status = RequestStatus.Rejected;
        request.RejectionReason = reason;
        await context.SaveChangesAsync();
        return request;
    }

    public async Task<ReplenishmentRequest?> FulfillRequestAsync(int id, List<ItemFulfillment> fulfilledItems)
    {
        var request = await context.Requests
                                    .Include(r => r.Items)
                                    .FirstOrDefaultAsync(r => r.Id == id);
                                    
        if (request == null || request.Status != RequestStatus.Approved) return null;

        foreach(var item in request.Items)
        {
            var frontendItem = fulfilledItems.FirstOrDefault(i => i.ArticleNumber == item.ArticleNumber);
            if (frontendItem != null)
            {
                item.FulfilledQuantity = frontendItem.FulfilledQuantity;
            }
        }

        request.Status = RequestStatus.Fulfilled;
        await context.SaveChangesAsync();
        
        return request;
    }
}