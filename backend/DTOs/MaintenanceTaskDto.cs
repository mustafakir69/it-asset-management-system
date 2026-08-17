namespace TakipProgrami.Api.DTOs;

public sealed record MaintenanceTaskDto(
    string Id,
    string MaintenancePlanId,
    string AssetId,
    string AssetCode,
    string AssetName,
    string Title,
    string? Description,
    DateOnly PlannedDate,
    DateOnly? CompletedDate,
    string Status,
    string DisplayStatus,
    string ResponsibleUserId,
    string ResponsibleUserName,
    string? Notes,
    string? CompletedByUserId,
    string? CompletedByName,
    string? Result,
    string? WorkNotes,
    string? CancellationReason,
    DateTimeOffset CreatedAt);
