namespace TakipProgrami.Api.Entities;

public enum MaintenanceRequestPriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum MaintenanceRequestStatus
{
    Open,
    Assigned,
    InProgress,
    Completed,
    Cancelled
}

public sealed class MaintenanceRequest
{
    public string Id { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string RequestedByEmployeeId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MaintenanceRequestPriority Priority { get; set; }
    public MaintenanceRequestStatus Status { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? CompletedByUserId { get; set; }
    public string? Result { get; set; }
    public string? WorkNotes { get; set; }
    public string? CancellationReason { get; set; }
    public Asset Asset { get; set; } = null!;
    public Employee RequestedByEmployee { get; set; } = null!;
    public AppUser? AssignedToUser { get; set; }
    public AppUser? CompletedByUser { get; set; }
}
