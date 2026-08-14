namespace TakipProgrami.Api.Entities;

public sealed class StockAlert
{
    public string Id { get; set; } = string.Empty;
    public string StockItemId { get; set; } = string.Empty;
    public DateTimeOffset TriggeredAt { get; set; }
    public int QuantityAtTrigger { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public DateTimeOffset? SentAt { get; set; }
    public NotificationDeliveryStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public StockItem StockItem { get; set; } = null!;
}
