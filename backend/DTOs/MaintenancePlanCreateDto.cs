using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public class MaintenancePlanCreateDto
{
    [Required(ErrorMessage = "Cihaz seçimi zorunludur.")]
    public string AssetId { get; init; } = string.Empty;

    [Required(ErrorMessage = "Bakım planı adı zorunludur.")]
    [StringLength(150, ErrorMessage = "Bakım planı adı en fazla 150 karakter olabilir.")]
    public string Name { get; init; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    public string? Description { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Bakım sıklığı 1 günden az olamaz.")]
    public int FrequencyDays { get; init; }

    [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
    public DateOnly? StartDate { get; init; }

    [Required(ErrorMessage = "Sorumlu IT personeli zorunludur.")]
    public string ResponsibleUserId { get; init; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "Tahmini süre 1-1440 dakika arasında olmalıdır.")]
    public int EstimatedDurationMinutes { get; init; }

    [Range(0, 365, ErrorMessage = "Hatırlatma günü 0-365 arasında olmalıdır.")]
    public int ReminderLeadDays { get; init; }
}
