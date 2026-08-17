using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed record AssignmentDto(
    string Id,
    string AssetId,
    string AssetCode,
    string AssetName,
    string AssetCategory,
    string AssetBrand,
    string AssetModel,
    string AssetStatus,
    string EmployeeId,
    string EmployeeNo,
    string EmployeeName,
    string Department,
    DateTimeOffset AssignedAt,
    DateTimeOffset? ReturnedAt,
    string AssignedByUserId,
    string AssignedByName,
    string? ReturnedByUserId,
    string? ReturnedByName,
    string? Notes,
    string? ReturnNotes,
    bool IsActive);

public sealed class AssignmentCreateDto
{
    [Required(ErrorMessage = "Cihaz zorunludur.")]
    public string AssetId { get; init; } = string.Empty;

    [Required(ErrorMessage = "Çalışan zorunludur.")]
    public string EmployeeId { get; init; } = string.Empty;

    [Required(ErrorMessage = "Zimmet tarihi zorunludur.")]
    public DateTimeOffset? AssignedAt { get; init; }

    [StringLength(1000, ErrorMessage = "Zimmet notu en fazla 1000 karakter olabilir.")]
    public string? Notes { get; init; }
}

public sealed class AssignmentReturnDto
{
    [Required(ErrorMessage = "İade tarihi zorunludur.")]
    public DateTimeOffset? ReturnedAt { get; init; }

    [StringLength(1000, ErrorMessage = "İade notu en fazla 1000 karakter olabilir.")]
    public string? ReturnNotes { get; init; }
}
