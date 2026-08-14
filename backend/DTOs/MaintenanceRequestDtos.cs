using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed record MaintenanceRequestDto(
    string Id,
    string RequestNumber,
    string AssetId,
    string AssetCode,
    string AssetName,
    string Title,
    string Description,
    string Priority,
    string Status,
    string RequestedBy,
    string? AssignedTechnician,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt,
    string? CompletedBy,
    string? Result,
    string? WorkNotes,
    string? CancellationReason);

public class MaintenanceRequestCreateDto
{
    [Required(ErrorMessage = "Cihaz seçimi zorunludur.")]
    public string AssetId { get; init; } = string.Empty;

    [Required(ErrorMessage = "Talep başlığı zorunludur.")]
    [StringLength(150, ErrorMessage = "Talep başlığı en fazla 150 karakter olabilir.")]
    public string Title { get; init; } = string.Empty;

    [Required(ErrorMessage = "Talep açıklaması zorunludur.")]
    [StringLength(2000, ErrorMessage = "Talep açıklaması en fazla 2000 karakter olabilir.")]
    public string Description { get; init; } = string.Empty;

    [Required(ErrorMessage = "Öncelik zorunludur.")]
    public string Priority { get; init; } = string.Empty;

    [Required(ErrorMessage = "Talebi açan kişi zorunludur.")]
    [StringLength(150, ErrorMessage = "Talebi açan kişi en fazla 150 karakter olabilir.")]
    public string RequestedBy { get; init; } = string.Empty;
}

public sealed class MaintenanceRequestUpdateDto : MaintenanceRequestCreateDto;

public sealed class MaintenanceRequestAssignDto
{
    [Required(ErrorMessage = "Atanan teknisyen zorunludur.")]
    [StringLength(150)]
    public string AssignedTechnician { get; init; } = string.Empty;
}

public sealed class MaintenanceRequestCompleteDto
{
    [Required(ErrorMessage = "Gerçekleşen tarih zorunludur.")]
    public DateTimeOffset? CompletedAt { get; init; }

    [Required(ErrorMessage = "Yapan kişi zorunludur.")]
    [StringLength(150)]
    public string CompletedBy { get; init; } = string.Empty;

    [Required(ErrorMessage = "Sonuç zorunludur.")]
    [StringLength(1000)]
    public string Result { get; init; } = string.Empty;

    [Required(ErrorMessage = "İşlem notu zorunludur.")]
    [StringLength(1000)]
    public string WorkNotes { get; init; } = string.Empty;
}

public sealed class MaintenanceRequestCancelDto
{
    [Required(ErrorMessage = "İptal nedeni zorunludur.")]
    [StringLength(1000)]
    public string CancellationReason { get; init; } = string.Empty;
}
