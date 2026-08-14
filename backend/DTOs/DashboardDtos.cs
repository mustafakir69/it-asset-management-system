namespace TakipProgrami.Api.DTOs;

public sealed record DashboardSummaryDto(
    int TotalAssets,
    int InStockAssets,
    int AssignedAssets,
    int MaintenanceAssets,
    int ExpiringWarranties,
    int ExpiringLicenses,
    int CriticalStockItems,
    int OverdueMaintenanceTasks,
    int OpenMaintenanceRequests,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<DashboardMovementDto> RecentMovements,
    IReadOnlyList<DashboardWarrantyDto> UpcomingWarranties,
    IReadOnlyList<DashboardStockDto> CriticalStock,
    IReadOnlyList<DashboardMaintenanceDto> UpcomingMaintenance);

public sealed record DashboardMovementDto(
    string AssignmentId,
    string AssetId,
    string AssetCode,
    string AssetName,
    string Description,
    string Status,
    DateTimeOffset OccurredAt);

public sealed record DashboardWarrantyDto(
    string AssetId,
    string AssetCode,
    string AssetName,
    DateOnly WarrantyEndDate,
    int RemainingDays,
    string Status);

public sealed record DashboardStockDto(
    string StockItemId,
    string ItemCode,
    string ItemName,
    int CurrentQuantity,
    int MinimumQuantity,
    string Unit,
    string Location);

public sealed record DashboardMaintenanceDto(
    string TaskId,
    string AssetCode,
    string AssetName,
    string Title,
    DateOnly PlannedDate,
    string Status);
