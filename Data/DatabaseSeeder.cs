using StockReplenishment.Models;

namespace StockReplenishment.Data;

public static class DatabaseSeeder
{
    public static void Initialize(AppDbContext context)
    {
        // Ensures the in-memory database is created
        context.Database.EnsureCreated();

        // If requests already exist, don't seed again
        if (context.Requests.Any())
        {
            return;
        }

        var seedRequests = new List<ReplenishmentRequest>
        {
            new ReplenishmentRequest
            {
                TargetLocation = "Assembly Line 1",
                Status = RequestStatus.Draft,
                Priority = RequestPriority.Normal,
                Items = new List<RequestItem>
                {
                    new RequestItem { ArticleNumber = "ART-100", Description = "M4 Screws", RequestedQuantity = 500 }
                }
            },
            new ReplenishmentRequest
            {
                TargetLocation = "Packaging Station A",
                Status = RequestStatus.Submitted,
                Priority = RequestPriority.Urgent,
                Items = new List<RequestItem>
                {
                    new RequestItem { ArticleNumber = "ART-205", Description = "Cardboard Boxes", RequestedQuantity = 150 },
                    new RequestItem { ArticleNumber = "ART-206", Description = "Packing Tape", RequestedQuantity = 20 }
                }
            },
            new ReplenishmentRequest
            {
                TargetLocation = "Welding Station 3",
                Status = RequestStatus.Approved,
                Priority = RequestPriority.Low,
                Items = new List<RequestItem>
                {
                    new RequestItem { ArticleNumber = "ART-900", Description = "Welding Wire", RequestedQuantity = 5 }
                }
            }
        };

        context.Requests.AddRange(seedRequests);
        context.SaveChanges();
    }
}