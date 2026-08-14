namespace TakipProgrami.Api.DTOs;

public sealed record StockTransactionDto(
    string Id,
    string StockItemId,
    string TransactionType,
    int Quantity,
    DateTimeOffset TransactionDate,
    string PersonName,
    string? Note);
