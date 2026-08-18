namespace TakipProgrami.Api.Entities;

public enum SupportRequestActivityType
{
    Created,
    Assigned,
    AssigneeChanged,
    Started,
    StatusChanged,
    SolutionAdded,
    Completed,
    Cancelled
}

public sealed class SupportRequestActivity
{
    public string Id { get; set; } = string.Empty;
    public string MaintenanceRequestId { get; set; } = string.Empty;
    public SupportRequestActivityType ActivityType { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string PerformedByUserId { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }

    public MaintenanceRequest MaintenanceRequest { get; set; } = null!;
    public AppUser PerformedByUser { get; set; } = null!;
}
