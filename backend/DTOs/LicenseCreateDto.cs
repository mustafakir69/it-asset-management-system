using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public class LicenseCreateDto : IValidatableObject
{
    [Required(ErrorMessage = "Lisans kodu zorunludur.")]
    [StringLength(50, ErrorMessage = "Lisans kodu en fazla 50 karakter olabilir.")]
    public string LicenseCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(150, ErrorMessage = "Ürün adı en fazla 150 karakter olabilir.")]
    public string ProductName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Sağlayıcı zorunludur.")]
    [StringLength(100, ErrorMessage = "Sağlayıcı en fazla 100 karakter olabilir.")]
    public string Vendor { get; init; } = string.Empty;

    [Required(ErrorMessage = "Lisans türü zorunludur.")]
    [StringLength(100, ErrorMessage = "Lisans türü en fazla 100 karakter olabilir.")]
    public string LicenseType { get; init; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Toplam lisans hakkı negatif olamaz.")]
    public int TotalSeats { get; init; }

    [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
    public DateOnly? StartDate { get; init; }

    public DateOnly? ExpirationDate { get; init; }

    public bool IsActive { get; init; }

    [StringLength(1000, ErrorMessage = "Not en fazla 1000 karakter olabilir.")]
    public string? Notes { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && ExpirationDate.HasValue && ExpirationDate < StartDate)
        {
            yield return new ValidationResult(
                "Bitiş tarihi başlangıç tarihinden önce olamaz.",
                [nameof(ExpirationDate)]);
        }
    }
}
