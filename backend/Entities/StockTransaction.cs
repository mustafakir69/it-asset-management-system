namespace TakipProgrami.Api.Entities;

public enum StockTransactionType
{
    Entry = 1,
    Exit = 2
}

public sealed class StockTransaction
{
    public string Id { get; set; } = string.Empty;
    public string StockItemId { get; set; } = string.Empty;
    public StockTransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public StockItem StockItem { get; set; } = null!;
}
