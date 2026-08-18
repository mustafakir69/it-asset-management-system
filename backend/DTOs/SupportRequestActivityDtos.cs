namespace TakipProgrami.Api.DTOs;

public sealed record SupportRequestActivityDto(
    string Id,
    string SupportRequestId,
    string ActivityType,
    DateTimeOffset OccurredAt,
    string PerformedByUserId,
    string PerformedByName,
    string? OldValue,
    string? NewValue,
    string? Description);
