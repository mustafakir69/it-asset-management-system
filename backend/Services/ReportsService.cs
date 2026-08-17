using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Services;

public sealed class ReportsService(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<InventoryReportDto>> GetInventoryAsync(
        string? category,
        string? status,
        string? location,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Assets.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(item => item.Category == category.Trim());
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(item => item.Status == status.Trim());
        if (!string.IsNullOrWhiteSpace(location)) query = query.Where(item => item.Location == location.Trim());

        return await query.OrderBy(item => item.AssetCode)
            .Select(item => new InventoryReportDto(
                item.AssetCode, item.Category, item.Brand, item.Model, item.SerialNumber,
                item.Status, item.Location, item.PurchaseDate, item.WarrantyEndDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssignmentReportDto>> GetAssignmentsAsync(
        string? status,
        string? department,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Assignments.AsNoTracking().Include(item => item.Asset).Include(item => item.Employee).Include(item => item.AssignedByUser).ThenInclude(x => x.Employee).Include(item => item.ReturnedByUser).ThenInclude(x => x!.Employee).AsQueryable();
        if (status?.Trim() == "Aktif") query = query.Where(item => item.ReturnedAt == null);
        if (status?.Trim() == "İade Edildi") query = query.Where(item => item.ReturnedAt != null);
        if (!string.IsNullOrWhiteSpace(department)) query = query.Where(item => item.Employee.Department == department.Trim());
        if (from.HasValue) query = query.Where(item => item.AssignedAt >= from.Value);
        if (to.HasValue) query = query.Where(item => item.AssignedAt <= to.Value);

        return await query.OrderByDescending(item => item.AssignedAt)
            .Select(item => new AssignmentReportDto(
                item.Asset.AssetCode,
                item.Asset.Brand + " " + item.Asset.Model,
                item.Employee.FullName,
                item.Employee.Department,
                item.AssignedAt,
                item.ReturnedAt,
                item.ReturnedAt == null ? "Aktif" : "İade Edildi",
                item.AssignedByUser.Employee != null ? item.AssignedByUser.Employee.FullName : item.AssignedByUser.Username,
                item.ReturnedByUser == null ? null : item.ReturnedByUser.Employee != null ? item.ReturnedByUser.Employee.FullName : item.ReturnedByUser.Username))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockReportDto>> GetStockAsync(
        string? category,
        string? location,
        bool? critical,
        CancellationToken cancellationToken)
    {
        var query = dbContext.StockItems.AsNoTracking().Where(item => item.IsActive);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(item => item.Category == category.Trim());
        if (!string.IsNullOrWhiteSpace(location)) query = query.Where(item => item.Location == location.Trim());
        if (critical.HasValue)
        {
            query = critical.Value
                ? query.Where(item => item.CurrentQuantity <= item.MinimumQuantity)
                : query.Where(item => item.CurrentQuantity > item.MinimumQuantity);
        }

        return await query.OrderBy(item => item.ItemCode)
            .Select(item => new StockReportDto(
                item.ItemCode, item.Name, item.Category, item.BrandModel, item.Unit,
                item.CurrentQuantity, item.MinimumQuantity,
                item.CurrentQuantity <= item.MinimumQuantity, item.Location))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockMovementReportDto>> GetStockMovementsAsync(
        CancellationToken cancellationToken) =>
        await dbContext.StockTransactions.AsNoTracking().Include(item => item.StockItem).Include(item => item.PerformedByUser).ThenInclude(x => x.Employee).Include(item => item.RecipientEmployee)
            .OrderByDescending(item => item.TransactionDate)
            .Select(item => new StockMovementReportDto(
                item.StockItem.ItemCode,
                item.StockItem.Name,
                item.TransactionType == StockTransactionType.Entry ? "Giriş" : "Çıkış",
                item.Quantity,
                item.TransactionDate,
                item.PerformedByUser.Employee != null ? item.PerformedByUser.Employee.FullName : item.PerformedByUser.Username,
                item.RecipientEmployee == null ? null : item.RecipientEmployee.FullName,
                item.Note))
            .ToListAsync(cancellationToken);

    public async Task<MaintenanceReportResponseDto> GetMaintenanceAsync(
        string? recordType,
        string? status,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tasks = await dbContext.MaintenanceTasks.AsNoTracking().Include(item => item.Asset).Include(item => item.MaintenancePlan).ThenInclude(x => x.ResponsibleUser).ThenInclude(x => x.Employee).Include(item => item.CompletedByUser).ThenInclude(x => x!.Employee)
            .ToListAsync(cancellationToken);
        var requests = await dbContext.MaintenanceRequests.AsNoTracking().Include(item => item.Asset).Include(item => item.AssignedToUser).ThenInclude(x => x!.Employee).Include(item => item.CompletedByUser).ThenInclude(x => x!.Employee)
            .ToListAsync(cancellationToken);

        var records = tasks.Select(task => new MaintenanceReportDto(
                task.Id,
                task.Asset.AssetCode,
                $"{task.Asset.Brand} {task.Asset.Model}",
                task.Title,
                "Periyodik Görev",
                task.PlannedDate,
                task.CompletedDate.HasValue
                    ? new DateTimeOffset(task.CompletedDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                    : null,
                task.CompletedByUser == null ? (task.MaintenancePlan.ResponsibleUser.Employee != null ? task.MaintenancePlan.ResponsibleUser.Employee.FullName : task.MaintenancePlan.ResponsibleUser.Username) : task.CompletedByUser.Employee != null ? task.CompletedByUser.Employee.FullName : task.CompletedByUser.Username,
                task.Result,
                TaskStatus(task, today)))
            .Concat(requests.Select(request => new MaintenanceReportDto(
                request.Id,
                request.Asset.AssetCode,
                $"{request.Asset.Brand} {request.Asset.Model}",
                request.Title,
                "Bakım Talebi",
                null,
                request.CompletedAt,
                request.CompletedByUser == null ? (request.AssignedToUser == null ? null : request.AssignedToUser.Employee != null ? request.AssignedToUser.Employee.FullName : request.AssignedToUser.Username) : request.CompletedByUser.Employee != null ? request.CompletedByUser.Employee.FullName : request.CompletedByUser.Username,
                request.Result,
                RequestStatus(request.Status))))
            .OrderByDescending(item => item.CompletedAt ??
                (item.PlannedDate.HasValue
                    ? new DateTimeOffset(item.PlannedDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                    : DateTimeOffset.MinValue))
            .ToList();

        if (!string.IsNullOrWhiteSpace(recordType))
            records = records.Where(item => item.RecordType == recordType.Trim()).ToList();
        if (!string.IsNullOrWhiteSpace(status))
            records = records.Where(item => item.Status == status.Trim()).ToList();

        var completedTasks = tasks.Where(item => item.Status == MaintenanceTaskStatus.Completed && item.CompletedDate.HasValue).ToList();
        var onTimeCount = completedTasks.Count(item => item.CompletedDate!.Value <= item.PlannedDate);
        var onTimeRate = completedTasks.Count == 0
            ? 0
            : Math.Round(onTimeCount * 100m / completedTasks.Count, 2);

        var summary = new MaintenanceReportSummaryDto(
            tasks.Count(item => item.Status == MaintenanceTaskStatus.Planned),
            tasks.Count(item => item.Status == MaintenanceTaskStatus.Completed) +
                requests.Count(item => item.Status == MaintenanceRequestStatus.Completed),
            tasks.Count(item => item.Status == MaintenanceTaskStatus.Planned && item.PlannedDate < today),
            tasks.Count(item => item.Status == MaintenanceTaskStatus.Cancelled) +
                requests.Count(item => item.Status == MaintenanceRequestStatus.Cancelled),
            onTimeRate);

        return new MaintenanceReportResponseDto(summary, records);
    }

    private static string TaskStatus(MaintenanceTask task, DateOnly today) => task.Status switch
    {
        MaintenanceTaskStatus.Completed => "Tamamlandı",
        MaintenanceTaskStatus.Cancelled => "İptal Edildi",
        _ when task.PlannedDate < today => "Gecikti",
        _ => "Planlandı"
    };

    private static string RequestStatus(MaintenanceRequestStatus status) => status switch
    {
        MaintenanceRequestStatus.Assigned => "Atandı",
        MaintenanceRequestStatus.InProgress => "İşlemde",
        MaintenanceRequestStatus.Completed => "Tamamlandı",
        MaintenanceRequestStatus.Cancelled => "İptal Edildi",
        _ => "Açık"
    };
}
