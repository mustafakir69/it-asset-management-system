namespace TakipProgrami.Api.Entities;

public enum MaintenanceTaskStatus
{
    Planned,
    Completed,
    Cancelled
}

public sealed class MaintenanceTask
{
    public string Id { get; set; } = string.Empty;
    public string MaintenancePlanId { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly PlannedDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public MaintenanceTaskStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? CompletedByUserId { get; set; }
    public string? Result { get; set; }
    public string? WorkNotes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public MaintenancePlan MaintenancePlan { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
    public AppUser? CompletedByUser { get; set; }
    public ICollection<MaintenanceNotification> Notifications { get; set; } = [];
}
