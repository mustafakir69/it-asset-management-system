using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed class MaintenanceTaskCompleteDto
{
    [Required(ErrorMessage = "Gerçekleşen tarih zorunludur.")]
    public DateOnly? CompletedDate { get; init; }

    [Required(ErrorMessage = "Bakım sonucu zorunludur.")]
    [StringLength(1000)]
    public string Result { get; init; } = string.Empty;

    [Required(ErrorMessage = "İşlem notu zorunludur.")]
    [StringLength(1000)]
    public string WorkNotes { get; init; } = string.Empty;
}

public sealed class MaintenanceTaskCancelDto
{
    [Required(ErrorMessage = "İptal nedeni zorunludur.")]
    [StringLength(1000)]
    public string CancellationReason { get; init; } = string.Empty;
}

public sealed class MaintenanceTaskRescheduleDto
{
    [Required(ErrorMessage = "Yeni planlanan tarih zorunludur.")]
    public DateOnly? PlannedDate { get; init; }

    [Required(ErrorMessage = "Yeniden planlama notu zorunludur.")]
    [StringLength(1000)]
    public string WorkNotes { get; init; } = string.Empty;
}
