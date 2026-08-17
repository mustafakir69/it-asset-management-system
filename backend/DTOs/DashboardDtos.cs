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

public sealed record EmployeeDashboardDto(
    int ActiveAssignmentCount,
    IReadOnlyList<EmployeeDashboardAssetDto> MyAssets,
    int OpenSupportRequestCount,
    int InProgressSupportRequestCount,
    IReadOnlyList<EmployeeDashboardSupportDto> RecentSupportRequests,
    EmployeeWarrantySummaryDto MyAssetsWarrantySummary);

public sealed record EmployeeDashboardAssetDto(string AssetId, string AssetCode, string AssetName, string Category, DateTimeOffset AssignedAt);
public sealed record EmployeeDashboardSupportDto(string Id, string RequestNumber, string Title, string Status, DateTimeOffset UpdatedAt);
public sealed record EmployeeWarrantySummaryDto(int Active, int ExpiringSoon, int Expired, int Unknown);
