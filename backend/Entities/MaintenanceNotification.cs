namespace TakipProgrami.Api.Entities;

public enum MaintenanceNotificationType
{
    Upcoming,
    Overdue
}

public sealed class MaintenanceNotification
{
    public string Id { get; set; } = string.Empty;
    public string MaintenanceTaskId { get; set; } = string.Empty;
    public MaintenanceNotificationType NotificationType { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public NotificationDeliveryStatus DeliveryStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public MaintenanceTask MaintenanceTask { get; set; } = null!;
}
