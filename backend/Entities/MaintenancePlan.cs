namespace TakipProgrami.Api.Entities;

public sealed class MaintenancePlan
{
    public string Id { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int FrequencyDays { get; set; }
    public DateOnly StartDate { get; set; }
    public string ResponsibleUserId { get; set; } = string.Empty;
    public int EstimatedDurationMinutes { get; set; }
    public int ReminderLeadDays { get; set; }
    public DateOnly NextDueAt { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Asset Asset { get; set; } = null!;
    public AppUser ResponsibleUser { get; set; } = null!;
    public ICollection<MaintenanceTask> Tasks { get; set; } = [];
}
