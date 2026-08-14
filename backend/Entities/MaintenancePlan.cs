namespace TakipProgrami.Api.Entities;

public sealed class MaintenancePlan
{
    public string Id { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int FrequencyDays { get; set; }
    public DateOnly StartDate { get; set; }
    public string ResponsibleTechnician { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Asset Asset { get; set; } = null!;
    public ICollection<MaintenanceTask> Tasks { get; set; } = [];
}
