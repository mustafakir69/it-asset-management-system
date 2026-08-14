namespace TakipProgrami.Api.DTOs;

public sealed record NotificationProcessResultDto(
    int StockAlertsCreated,
    int StockAlertsResolved,
    int MaintenanceNotificationsCreated,
    int Sent,
    int Skipped,
    int Failed,
    DateTimeOffset ProcessedAt);
