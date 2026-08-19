namespace StockReplenishment.Models;

public class ItemFulfillment
{
    public string ArticleNumber { get; set; } = string.Empty;
    public int FulfilledQuantity { get; set; }
}