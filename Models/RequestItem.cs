namespace StockReplenishment.Models;

public class RequestItem
{
    public int Id { get; set; }
    public int ReplenishmentRequestId { get; set; }
    
    public required string ArticleNumber { get; set; }
    public required string Description { get; set; }
    public int RequestedQuantity { get; set; }
    public int? FulfilledQuantity { get; set; } 
}