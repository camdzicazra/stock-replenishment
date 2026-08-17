namespace StockReplenishment.Models;

public enum RequestStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected,
    Fulfilled
}

public enum RequestPriority
{
    Low,
    Normal,
    Urgent
}