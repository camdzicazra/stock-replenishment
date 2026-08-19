using StockReplenishment.Models;

namespace StockReplenishment.Data;

public static class DatabaseSeeder
{
    public static void Initialize(AppDbContext context)
    {
        // Check if database already has data
        if (context.Requests.Any())
        {
            return; 
        }

        var requests = new List<ReplenishmentRequest>
        {
            new ReplenishmentRequest
            {
                TargetLocation = "Warehouse A",
                Status = RequestStatus.Draft,
                Priority = RequestPriority.Normal,
                Items = new List<RequestItem> 
                { 
                    new RequestItem { ArticleNumber = "ART-001", Description = "Safety Goggles", RequestedQuantity = 20 } 
                }
            },
            new ReplenishmentRequest
            {
                TargetLocation = "Store Front",
                Status = RequestStatus.Draft,
                Priority = RequestPriority.Low,
                Items = new List<RequestItem> 
                { 
                    new RequestItem { ArticleNumber = "ART-002", Description = "Display Racks", RequestedQuantity = 5 } 
                }
            },

            new ReplenishmentRequest
            {
                TargetLocation = "Warehouse B",
                Status = RequestStatus.Submitted,
                Priority = RequestPriority.Urgent,
                Items = new List<RequestItem> 
                { 
                    new RequestItem { ArticleNumber = "ART-003", Description = "Forklift Battery", RequestedQuantity = 2 } 
                }
            },
            new ReplenishmentRequest
            {
                TargetLocation = "Sector 7G",
                Status = RequestStatus.Submitted,
                Priority = RequestPriority.Normal,
                Items = new List<RequestItem> 
                { 
                    new RequestItem { ArticleNumber = "ART-004", Description = "Office Chairs", RequestedQuantity = 10 },
                    new RequestItem { ArticleNumber = "ART-005", Description = "Ergonomic Keyboards", RequestedQuantity = 10 }
                }
            },

            new ReplenishmentRequest
            {
                TargetLocation = "Loading Dock",
                Status = RequestStatus.Approved,
                Priority = RequestPriority.Urgent,
                Items = new List<RequestItem> 
                { 
                    new RequestItem { ArticleNumber = "ART-006", Description = "Pallet Jack", RequestedQuantity = 1 } 
                }
            },
            new ReplenishmentRequest
            {
                TargetLocation = "Store Front",
                Status = RequestStatus.Approved,
                Priority = RequestPriority.Low,
                Items = new List<RequestItem> 
                { 
                    new RequestItem { ArticleNumber = "ART-007", Description = "Cash Register Paper Rolls", RequestedQuantity = 50 } 
                }
            },

            new ReplenishmentRequest
            {
                TargetLocation = "Warehouse A",
                Status = RequestStatus.Rejected,
                Priority = RequestPriority.Normal,
                RejectionReason = "Quarterly budget exceeded for warehouse supplies. Please delay until next month.",
                Items = new List<RequestItem> 
                { 
                    new RequestItem { ArticleNumber = "ART-008", Description = "Laser Printer", RequestedQuantity = 3 } 
                }
            },
            new ReplenishmentRequest
            {
                TargetLocation = "Backroom",
                Status = RequestStatus.Rejected,
                Priority = RequestPriority.Urgent,
                RejectionReason = "Item ART-009 is currently discontinued by the supplier.",
                Items = new List<RequestItem> 
                { 
                    new RequestItem { ArticleNumber = "ART-009", Description = "Specialty Halogen Bulbs", RequestedQuantity = 15 } 
                }
            },

            new ReplenishmentRequest
            {
                TargetLocation = "Warehouse C",
                Status = RequestStatus.Fulfilled,
                Priority = RequestPriority.Normal,
                Items = new List<RequestItem> 
                { 
                    new RequestItem { ArticleNumber = "ART-010", Description = "Packing Tape (Cases)", RequestedQuantity = 10 } 
                }
            },
            new ReplenishmentRequest
            {
                TargetLocation = "Loading Dock",
                Status = RequestStatus.Fulfilled,
                Priority = RequestPriority.Urgent,
                Items = new List<RequestItem> 
                { 
                    new RequestItem { ArticleNumber = "ART-011", Description = "Spill Cleanup Kit", RequestedQuantity = 4 } 
                }
            }
        };

        context.Requests.AddRange(requests);
        context.SaveChanges();
    }
}