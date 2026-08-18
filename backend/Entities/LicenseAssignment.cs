namespace TakipProgrami.Api.Entities;

public sealed class LicenseAssignment
{
    public string Id { get; set; } = string.Empty;
    public string LicenseId { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string? AssetId { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
    public string AssignedByUserId { get; set; } = string.Empty;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedByUserId { get; set; }

    public License License { get; set; } = null!;
    public Employee? Employee { get; set; }
    public Asset? Asset { get; set; }
    public AppUser AssignedByUser { get; set; } = null!;
    public AppUser? RevokedByUser { get; set; }
}
