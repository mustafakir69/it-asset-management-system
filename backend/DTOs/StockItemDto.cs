namespace TakipProgrami.Api.DTOs;

public sealed record StockItemDto(
    string Id,
    string ItemCode,
    string Name,
    string Category,
    string BrandModel,
    string Unit,
    int CurrentQuantity,
    int MinimumQuantity,
    string Location,
    bool IsActive,
    bool IsCritical);
