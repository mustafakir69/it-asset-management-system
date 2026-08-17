namespace TakipProgrami.Api.DTOs;

public sealed record MaintenancePlanDto(
    string Id,
    string AssetId,
    string AssetCode,
    string AssetName,
    string Name,
    string? Description,
    int FrequencyDays,
    DateOnly StartDate,
    string ResponsibleUserId,
    string ResponsibleUserName,
    int EstimatedDurationMinutes,
    int ReminderLeadDays,
    DateOnly NextDueAt,
    bool IsActive,
    DateTimeOffset CreatedAt);
