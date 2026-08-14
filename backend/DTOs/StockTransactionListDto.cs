namespace TakipProgrami.Api.DTOs;

public sealed record StockTransactionListDto(
    string Id,
    string StockItemId,
    string ItemCode,
    string ItemName,
    string TransactionType,
    int Quantity,
    DateTimeOffset TransactionDate,
    string PersonName,
    string? Note);
