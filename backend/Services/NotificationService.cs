using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Services;

public sealed class NotificationService(
    ApplicationDbContext dbContext,
    IEmailService emailService,
    IOptions<EmailOptions> options,
    ILogger<NotificationService> logger)
{
    private readonly EmailOptions emailOptions = options.Value;

    public async Task<StockAlert?> SyncStockAlertAsync(
        StockItem stockItem,
        CancellationToken cancellationToken)
    {
        var activeAlert = await dbContext.StockAlerts
            .FirstOrDefaultAsync(
                alert => alert.StockItemId == stockItem.Id && alert.ResolvedAt == null,
                cancellationToken);
        var isCritical = stockItem.CurrentQuantity <= stockItem.MinimumQuantity;

        if (!isCritical)
        {
            if (activeAlert is not null)
            {
                activeAlert.ResolvedAt = DateTimeOffset.UtcNow;
                activeAlert.Status = NotificationDeliveryStatus.Resolved;
            }
            return null;
        }

        if (activeAlert is not null) return null;

        var alert = new StockAlert
        {
            Id = Guid.NewGuid().ToString("N"),
            StockItemId = stockItem.Id,
            StockItem = stockItem,
            TriggeredAt = DateTimeOffset.UtcNow,
            QuantityAtTrigger = stockItem.CurrentQuantity,
            Recipient = emailOptions.StockRecipient.Trim(),
            Status = NotificationDeliveryStatus.Pending
        };
        dbContext.StockAlerts.Add(alert);
        return alert;
    }

    public async Task DeliverStockAlertAsync(
        StockAlert alert,
        StockItem stockItem,
        CancellationToken cancellationToken)
    {
        var subject = $"Kritik stok uyarısı: {stockItem.ItemCode}";
        var body = $"Ürün kodu: {stockItem.ItemCode}\nÜrün adı: {stockItem.Name}\nMevcut miktar: {stockItem.CurrentQuantity} {stockItem.Unit}\nMinimum miktar: {stockItem.MinimumQuantity} {stockItem.Unit}\nLokasyon: {stockItem.Location}\nKritik durum tarihi: {alert.TriggeredAt:dd.MM.yyyy HH:mm}";
        ApplyDeliveryResult(alert, await emailService.SendAsync(
            alert.Recipient, subject, body, cancellationToken));
    }

    public async Task<NotificationProcessResultDto> ProcessAsync(
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var createdStockAlerts = new List<(StockAlert Alert, StockItem Item)>();
        var resolvedBefore = await dbContext.StockAlerts.CountAsync(
            alert => alert.ResolvedAt != null,
            cancellationToken);
        var stockItems = await dbContext.StockItems
            .Where(item => item.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var item in stockItems)
        {
            var alert = await SyncStockAlertAsync(item, cancellationToken);
            if (alert is not null) createdStockAlerts.Add((alert, item));
        }

        var existingKeys = await dbContext.MaintenanceNotifications
            .Select(item => new { item.MaintenanceTaskId, item.NotificationType })
            .ToListAsync(cancellationToken);
        var keySet = existingKeys
            .Select(item => $"{item.MaintenanceTaskId}:{item.NotificationType}")
            .ToHashSet(StringComparer.Ordinal);
        var tasks = await dbContext.MaintenanceTasks
            .Include(item => item.Asset)
            .Include(item => item.MaintenancePlan).ThenInclude(item => item.ResponsibleUser).ThenInclude(item => item.Employee)
            .Where(item => item.Status == MaintenanceTaskStatus.Planned)
            .ToListAsync(cancellationToken);
        var createdMaintenance = new List<(MaintenanceNotification Notification, MaintenanceTask Task)>();
        foreach (var task in tasks)
        {
            var notificationType = MaintenanceRules.GetNotificationType(task, today);
            if (!notificationType.HasValue) continue;
            var key = $"{task.Id}:{notificationType.Value}";
            if (!keySet.Add(key)) continue;

            var notification = new MaintenanceNotification
            {
                Id = Guid.NewGuid().ToString("N"),
                MaintenanceTaskId = task.Id,
                MaintenanceTask = task,
                NotificationType = notificationType.Value,
                Recipient = emailOptions.MaintenanceRecipient.Trim(),
                ScheduledAt = DateTimeOffset.UtcNow,
                DeliveryStatus = NotificationDeliveryStatus.Pending
            };
            dbContext.MaintenanceNotifications.Add(notification);
            createdMaintenance.Add((notification, task));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var (alert, item) in createdStockAlerts)
            await DeliverStockAlertAsync(alert, item, cancellationToken);
        foreach (var (notification, task) in createdMaintenance)
            await DeliverMaintenanceNotificationAsync(notification, task, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        var delivered = createdStockAlerts.Select(item => item.Alert.Status)
            .Concat(createdMaintenance.Select(item => item.Notification.DeliveryStatus))
            .ToList();
        var resolvedAfter = await dbContext.StockAlerts.CountAsync(
            alert => alert.ResolvedAt != null,
            cancellationToken);

        logger.LogInformation(
            "Notification processing completed. StockCreated={StockCreated}, StockResolved={StockResolved}, MaintenanceCreated={MaintenanceCreated}",
            createdStockAlerts.Count,
            resolvedAfter - resolvedBefore,
            createdMaintenance.Count);
        return new(
            createdStockAlerts.Count,
            resolvedAfter - resolvedBefore,
            createdMaintenance.Count,
            delivered.Count(status => status == NotificationDeliveryStatus.Sent),
            delivered.Count(status => status == NotificationDeliveryStatus.Skipped),
            delivered.Count(status => status == NotificationDeliveryStatus.Failed),
            DateTimeOffset.UtcNow);
    }

    private async Task DeliverMaintenanceNotificationAsync(
        MaintenanceNotification notification,
        MaintenanceTask task,
        CancellationToken cancellationToken)
    {
        var typeText = notification.NotificationType == MaintenanceNotificationType.Upcoming
            ? "Yaklaşan bakım"
            : "Geciken bakım";
        var subject = $"{typeText}: {task.Asset.AssetCode}";
        var responsible = task.MaintenancePlan.ResponsibleUser.Employee?.FullName ?? task.MaintenancePlan.ResponsibleUser.Username;
        var body = $"Cihaz: {task.Asset.AssetCode} - {task.Asset.Brand} {task.Asset.Model}\nBakım: {task.Title}\nPlanlanan tarih: {task.PlannedDate:dd.MM.yyyy}\nSorumlu: {responsible}\nBildirim türü: {typeText}";
        ApplyDeliveryResult(notification, await emailService.SendAsync(
            notification.Recipient, subject, body, cancellationToken));
    }

    private static void ApplyDeliveryResult(StockAlert alert, EmailSendResult result)
    {
        alert.Status = Map(result.Status);
        alert.ErrorMessage = result.ErrorMessage;
        alert.SentAt = result.Status == EmailSendStatus.Sent ? DateTimeOffset.UtcNow : null;
    }

    private static void ApplyDeliveryResult(
        MaintenanceNotification notification,
        EmailSendResult result)
    {
        notification.DeliveryStatus = Map(result.Status);
        notification.ErrorMessage = result.ErrorMessage;
        notification.SentAt = result.Status == EmailSendStatus.Sent
            ? DateTimeOffset.UtcNow
            : null;
    }

    private static NotificationDeliveryStatus Map(EmailSendStatus status) => status switch
    {
        EmailSendStatus.Sent => NotificationDeliveryStatus.Sent,
        EmailSendStatus.Failed => NotificationDeliveryStatus.Failed,
        _ => NotificationDeliveryStatus.Skipped
    };
}
