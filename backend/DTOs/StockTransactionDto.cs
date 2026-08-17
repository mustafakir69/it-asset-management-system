namespace TakipProgrami.Api.DTOs;

public sealed record StockTransactionDto(
    string Id,
    string StockItemId,
    string TransactionType,
    int Quantity,
    DateTimeOffset TransactionDate,
    string PerformedByUserId,
    string PerformedByName,
    string? RecipientEmployeeId,
    string? RecipientEmployeeName,
    string? Note);
