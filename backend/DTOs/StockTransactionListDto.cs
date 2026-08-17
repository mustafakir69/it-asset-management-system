namespace TakipProgrami.Api.DTOs;

public sealed record StockTransactionListDto(
    string Id,
    string StockItemId,
    string ItemCode,
    string ItemName,
    string TransactionType,
    int Quantity,
    DateTimeOffset TransactionDate,
    string PerformedByUserId,
    string PerformedByName,
    string? RecipientEmployeeId,
    string? RecipientEmployeeName,
    string? Note);
