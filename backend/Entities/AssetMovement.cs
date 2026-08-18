namespace TakipProgrami.Api.Entities;

public enum AssetMovementType
{
    InventoryCreated,
    InformationUpdated,
    Assigned,
    Returned,
    MaintenanceStarted,
    MaintenanceCompleted,
    StatusChanged,
    MarkedLost,
    Scrapped,
    Disposed
}

public sealed class AssetMovement
{
    public string Id { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public AssetMovementType MovementType { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string PerformedByUserId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public string? Method { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Asset Asset { get; set; } = null!;
    public AppUser PerformedByUser { get; set; } = null!;
}
