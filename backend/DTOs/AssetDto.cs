namespace TakipProgrami.Api.DTOs;

public sealed record AssetDto(
    string Id,
    string AssetCode,
    string Category,
    string Brand,
    string Model,
    string SerialNumber,
    string Status,
    string Location,
    DateOnly PurchaseDate,
    DateOnly WarrantyEndDate);
