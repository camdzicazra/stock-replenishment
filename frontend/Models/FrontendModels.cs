namespace frontend.Models;

public enum RequestStatus { Draft, Submitted, Approved, Rejected, Fulfilled }
public enum RequestPriority { Low, Normal, Urgent }

public class RequestItemDto
{
    public int Id { get; set; }
    public string ArticleNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int? FulfilledQuantity { get; set; }
}

public class ReplenishmentRequestDto
{
    public int Id { get; set; }
    public string TargetLocation { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public RequestPriority Priority { get; set; }
    public string? RejectionReason { get; set; }
    public List<RequestItemDto> Items { get; set; } = new();
}

public class PagedResultDto<T>
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<T> Items { get; set; } = new();
}