namespace StockReplenishment.Models;

public class ReplenishmentRequest
{
    public int Id { get; set; }
    public required string TargetLocation { get; set; }
    
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public RequestPriority Priority { get; set; } = RequestPriority.Normal;
    
    public string? RejectionReason { get; set; }
    
    public List<RequestItem> Items { get; set; } = new();
}