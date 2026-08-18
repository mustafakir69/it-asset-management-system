using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Helpers;

public static class AssetMovementFactory
{
    public static AssetMovement Create(
        string assetId,
        AssetMovementType movementType,
        DateTimeOffset occurredAt,
        string? previousStatus,
        string newStatus,
        string performedByUserId,
        string? description = null,
        string? reason = null,
        string? method = null,
        string? relatedEntityType = null,
        string? relatedEntityId = null) =>
        new()
        {
            Id = Guid.NewGuid().ToString("N"),
            AssetId = assetId,
            MovementType = movementType,
            OccurredAt = occurredAt,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            PerformedByUserId = performedByUserId,
            Description = Clean(description),
            Reason = Clean(reason),
            Method = Clean(method),
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            CreatedAt = DateTimeOffset.UtcNow
        };

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
