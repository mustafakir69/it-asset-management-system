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
    string ResponsibleTechnician,
    bool IsActive,
    DateTimeOffset CreatedAt);
