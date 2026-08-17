using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Services;

public sealed class DashboardService(ApplicationDbContext dbContext)
{
    public async Task<EmployeeDashboardDto> GetMySummaryAsync(string employeeId, CancellationToken cancellationToken)
    {
        var assignments = await dbContext.Assignments.AsNoTracking()
            .Where(x => x.EmployeeId == employeeId && x.ReturnedAt == null)
            .Select(x => new { x.AssetId, x.Asset.AssetCode, AssetName = x.Asset.Brand + " " + x.Asset.Model, x.Asset.Category, x.Asset.WarrantyEndDate, x.AssignedAt })
            .ToListAsync(cancellationToken);
        var requests = await dbContext.MaintenanceRequests.AsNoTracking().Where(x => x.RequestedByEmployeeId == employeeId)
            .OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.Today); var warning = today.AddDays(30);
        return new EmployeeDashboardDto(assignments.Count,
            assignments.Select(x => new EmployeeDashboardAssetDto(x.AssetId, x.AssetCode, x.AssetName, x.Category, x.AssignedAt)).ToList(),
            requests.Count(x => x.Status is MaintenanceRequestStatus.Open or MaintenanceRequestStatus.Assigned),
            requests.Count(x => x.Status == MaintenanceRequestStatus.InProgress),
            requests.Take(5).Select(x => new EmployeeDashboardSupportDto(x.Id, $"BT-{x.Id[..Math.Min(8, x.Id.Length)].ToUpperInvariant()}", x.Title, x.Status switch { MaintenanceRequestStatus.Assigned => "Atandı", MaintenanceRequestStatus.InProgress => "İşlemde", MaintenanceRequestStatus.Completed => "Tamamlandı", MaintenanceRequestStatus.Cancelled => "İptal Edildi", _ => "Açık" }, x.UpdatedAt)).ToList(),
            new EmployeeWarrantySummaryDto(
                assignments.Count(x => x.WarrantyEndDate > warning),
                assignments.Count(x => x.WarrantyEndDate >= today && x.WarrantyEndDate <= warning),
                assignments.Count(x => x.WarrantyEndDate < today),
                assignments.Count(x => x.WarrantyEndDate == null)));
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var warningDate = today.AddDays(30);

        var totalAssets = await dbContext.Assets.CountAsync(cancellationToken);
        var inStockAssets = await dbContext.Assets.CountAsync(item => item.Status == "Stokta", cancellationToken);
        var assignedAssets = await dbContext.Assets.CountAsync(item => item.Status == "Zimmetli", cancellationToken);
        var maintenanceAssets = await dbContext.Assets.CountAsync(item => item.Status == "Bakımda", cancellationToken);
        var expiringWarranties = await dbContext.Assets.CountAsync(
            item => item.WarrantyEndDate >= today && item.WarrantyEndDate <= warningDate,
            cancellationToken);
        var expiringLicenses = await dbContext.Licenses.CountAsync(
            item => item.IsActive && item.ExpirationDate >= today && item.ExpirationDate <= warningDate,
            cancellationToken);
        var criticalStockItems = await dbContext.StockItems.CountAsync(
            item => item.IsActive && item.CurrentQuantity <= item.MinimumQuantity,
            cancellationToken);
        var overdueMaintenanceTasks = await dbContext.MaintenanceTasks.CountAsync(
            item => item.Status == MaintenanceTaskStatus.Planned && item.PlannedDate < today,
            cancellationToken);
        var openMaintenanceRequests = await dbContext.MaintenanceRequests.CountAsync(
            item => item.Status != MaintenanceRequestStatus.Completed && item.Status != MaintenanceRequestStatus.Cancelled,
            cancellationToken);

        var assignments = await dbContext.Assignments
            .AsNoTracking()
            .Include(item => item.Asset)
            .OrderByDescending(item => item.ReturnedAt ?? item.AssignedAt)
            .Take(8)
            .ToListAsync(cancellationToken);
        var movements = assignments
            .SelectMany(item => new[]
            {
                new DashboardMovementDto(
                    item.Id,
                    item.AssetId,
                    item.Asset.AssetCode,
                    $"{item.Asset.Brand} {item.Asset.Model}",
                    $"{item.Asset.AssetCode} cihazı zimmetlendi.",
                    "Zimmetli",
                    item.AssignedAt),
                item.ReturnedAt.HasValue
                    ? new DashboardMovementDto(
                        item.Id,
                        item.AssetId,
                        item.Asset.AssetCode,
                        $"{item.Asset.Brand} {item.Asset.Model}",
                        $"{item.Asset.AssetCode} cihazı iade alındı.",
                        "Stokta",
                        item.ReturnedAt.Value)
                    : null
            })
            .Where(item => item is not null)
            .Cast<DashboardMovementDto>()
            .OrderByDescending(item => item.OccurredAt)
            .Take(6)
            .ToList();

        var warranties = await dbContext.Assets
            .AsNoTracking()
            .Where(item => item.WarrantyEndDate >= today && item.WarrantyEndDate <= warningDate)
            .OrderBy(item => item.WarrantyEndDate)
            .Take(6)
            .Select(item => new DashboardWarrantyDto(
                item.Id,
                item.AssetCode,
                item.Brand + " " + item.Model,
                item.WarrantyEndDate!.Value,
                item.WarrantyEndDate.Value.DayNumber - today.DayNumber,
                "Yaklaşıyor"))
            .ToListAsync(cancellationToken);

        var criticalStock = await dbContext.StockItems
            .AsNoTracking()
            .Where(item => item.IsActive && item.CurrentQuantity <= item.MinimumQuantity)
            .OrderBy(item => item.CurrentQuantity - item.MinimumQuantity)
            .Take(6)
            .Select(item => new DashboardStockDto(
                item.Id,
                item.ItemCode,
                item.Name,
                item.CurrentQuantity,
                item.MinimumQuantity,
                item.Unit,
                item.Location))
            .ToListAsync(cancellationToken);

        var maintenance = await dbContext.MaintenanceTasks
            .AsNoTracking()
            .Include(item => item.Asset)
            .Where(item =>
                item.Status == MaintenanceTaskStatus.Planned &&
                item.PlannedDate <= warningDate)
            .OrderBy(item => item.PlannedDate)
            .Take(6)
            .Select(item => new DashboardMaintenanceDto(
                item.Id,
                item.Asset.AssetCode,
                item.Asset.Brand + " " + item.Asset.Model,
                item.Title,
                item.PlannedDate,
                item.PlannedDate < today ? "Gecikmiş" : "Yaklaşıyor"))
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto(
            totalAssets,
            inStockAssets,
            assignedAssets,
            maintenanceAssets,
            expiringWarranties,
            expiringLicenses,
            criticalStockItems,
            overdueMaintenanceTasks,
            openMaintenanceRequests,
            DateTimeOffset.UtcNow,
            movements,
            warranties,
            criticalStock,
            maintenance);
    }
}
