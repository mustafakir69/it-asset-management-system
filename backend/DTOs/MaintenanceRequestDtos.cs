using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed record MaintenanceRequestDto(
    string Id, string RequestNumber, string AssetId, string AssetCode, string AssetName,
    string RequestedByEmployeeId, string RequestedByName, string RequestedByDepartment,
    string Title, string Description, string Priority, string Status,
    string? AssignedToUserId, string? AssignedToName,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, DateTimeOffset? CompletedAt,
    string? CompletedByUserId, string? CompletedByName, string? Result,
    string? WorkNotes, string? CancellationReason);

public sealed class MaintenanceRequestCreateDto
{
    [Required(ErrorMessage = "Cihaz seçimi zorunludur.")]
    public string AssetId { get; init; } = string.Empty;
    [Required, StringLength(150)] public string Title { get; init; } = string.Empty;
    [Required, StringLength(2000)] public string Description { get; init; } = string.Empty;
    [Required] public string Priority { get; init; } = string.Empty;
}

public sealed class MaintenanceRequestAssignDto
{
    [Required(ErrorMessage = "Aktif bir IT personeli seçin.")]
    public string AssignedToUserId { get; init; } = string.Empty;
}

public sealed class MaintenanceRequestCompleteDto
{
    [Required] public DateTimeOffset? CompletedAt { get; init; }
    [Required, StringLength(1000)] public string Result { get; init; } = string.Empty;
    [Required, StringLength(1000)] public string WorkNotes { get; init; } = string.Empty;
}

public sealed class MaintenanceRequestCancelDto
{
    [Required, StringLength(1000)] public string CancellationReason { get; init; } = string.Empty;
}
