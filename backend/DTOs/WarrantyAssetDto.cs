namespace TakipProgrami.Api.DTOs;

public sealed record WarrantyAssetDto(
    string AssetId,
    string AssetCode,
    string Category,
    string Brand,
    string Model,
    string SerialNumber,
    string Location,
    DateOnly PurchaseDate,
    DateOnly? WarrantyEndDate,
    int? RemainingDays,
    string WarrantyStatus);
