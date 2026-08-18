using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed record LicenseAssignmentDto(
    string Id,
    string LicenseId,
    string LicenseCode,
    string ProductName,
    string LicenseType,
    string? EmployeeId,
    string? EmployeeName,
    string? EmployeeDepartment,
    string? AssetId,
    string? AssetCode,
    string? AssetName,
    DateTimeOffset AssignedAt,
    string AssignedByUserId,
    string AssignedByName,
    DateTimeOffset? RevokedAt,
    string? RevokedByUserId,
    string? RevokedByName,
    string Status);

public sealed class LicenseAssignmentCreateDto : IValidatableObject
{
    public string? EmployeeId { get; init; }
    public string? AssetId { get; init; }

    [Required(ErrorMessage = "Atama tarihi zorunludur.")]
    public DateTimeOffset? AssignedAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(EmployeeId) && string.IsNullOrWhiteSpace(AssetId))
        {
            yield return new ValidationResult(
                "En az bir çalışan veya cihaz seçilmelidir.",
                [nameof(EmployeeId), nameof(AssetId)]);
        }
    }
}
