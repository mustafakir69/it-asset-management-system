namespace TakipProgrami.Api.DTOs;

public sealed record InventoryReportDto(
    string AssetCode,
    string Category,
    string Brand,
    string Model,
    string SerialNumber,
    string Status,
    string Location,
    DateOnly PurchaseDate,
    DateOnly? WarrantyEndDate);

public sealed record AssignmentReportDto(
    string AssetCode,
    string AssetName,
    string EmployeeName,
    string Department,
    DateTimeOffset AssignedAt,
    DateTimeOffset? ReturnedAt,
    string Status,
    string AssignedByName,
    string? ReturnedByName);

public sealed record StockReportDto(
    string ItemCode,
    string ItemName,
    string Category,
    string BrandModel,
    string Unit,
    int CurrentQuantity,
    int MinimumQuantity,
    bool IsCritical,
    string Location);

public sealed record StockMovementReportDto(
    string ItemCode,
    string ItemName,
    string TransactionType,
    int Quantity,
    DateTimeOffset TransactionDate,
    string PerformedByName,
    string? RecipientEmployeeName,
    string? Note);

public sealed record MaintenanceReportDto(
    string Id,
    string AssetCode,
    string AssetName,
    string Title,
    string RecordType,
    DateOnly? PlannedDate,
    DateTimeOffset? CompletedAt,
    string? ActorName,
    string? Result,
    string Status);

public sealed record MaintenanceReportSummaryDto(
    int Planned,
    int Completed,
    int Overdue,
    int Cancelled,
    decimal OnTimeCompletionRate);

public sealed record MaintenanceReportResponseDto(
    MaintenanceReportSummaryDto Summary,
    IReadOnlyList<MaintenanceReportDto> Records);

public sealed record WarrantyReportDto(
    string AssetCode,
    string AssetName,
    string Category,
    DateOnly? WarrantyEndDate,
    string WarrantyStatus,
    string AssetStatus,
    string? CurrentAssigneeName,
    string? CurrentAssigneeDepartment);

public sealed record LicenseReportDto(
    string LicenseCode,
    string ProductName,
    string Vendor,
    string LicenseType,
    int TotalSeats,
    int UsedSeats,
    int AvailableSeats,
    DateOnly? ExpirationDate,
    string Status);

public sealed record SupportReportDto(
    string RequestNumber,
    string AssetCode,
    string AssetName,
    string RequestedByName,
    string Department,
    string Priority,
    string Status,
    string? AssignedToName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    string? CompletedByName,
    string? Result);
