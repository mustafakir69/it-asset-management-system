namespace TakipProgrami.Api.Entities;

public sealed class Assignment
{
    public string Id { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? ReturnedAt { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
    public string? ReturnedBy { get; set; }
    public string? Notes { get; set; }
    public string? ReturnNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Asset Asset { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
