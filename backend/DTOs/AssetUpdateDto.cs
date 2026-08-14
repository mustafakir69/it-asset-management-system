using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed class AssetUpdateDto : IValidatableObject
{
    [Required(ErrorMessage = "Varlık kodu zorunludur.")]
    [StringLength(50, ErrorMessage = "Varlık kodu en fazla 50 karakter olabilir.")]
    public string AssetCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "Kategori zorunludur.")]
    [StringLength(100, ErrorMessage = "Kategori en fazla 100 karakter olabilir.")]
    public string Category { get; init; } = string.Empty;

    [Required(ErrorMessage = "Marka zorunludur.")]
    [StringLength(100, ErrorMessage = "Marka en fazla 100 karakter olabilir.")]
    public string Brand { get; init; } = string.Empty;

    [Required(ErrorMessage = "Model zorunludur.")]
    [StringLength(150, ErrorMessage = "Model en fazla 150 karakter olabilir.")]
    public string Model { get; init; } = string.Empty;

    [Required(ErrorMessage = "Seri numarası zorunludur.")]
    [StringLength(100, ErrorMessage = "Seri numarası en fazla 100 karakter olabilir.")]
    public string SerialNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Durum zorunludur.")]
    [StringLength(50, ErrorMessage = "Durum en fazla 50 karakter olabilir.")]
    public string Status { get; init; } = string.Empty;

    [Required(ErrorMessage = "Lokasyon zorunludur.")]
    [StringLength(150, ErrorMessage = "Lokasyon en fazla 150 karakter olabilir.")]
    public string Location { get; init; } = string.Empty;

    [Required(ErrorMessage = "Satın alma tarihi zorunludur.")]
    public DateOnly? PurchaseDate { get; init; }

    [Required(ErrorMessage = "Garanti bitiş tarihi zorunludur.")]
    public DateOnly? WarrantyEndDate { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PurchaseDate.HasValue && WarrantyEndDate.HasValue && WarrantyEndDate < PurchaseDate)
        {
            yield return new ValidationResult(
                "Garanti bitiş tarihi satın alma tarihinden önce olamaz.",
                [nameof(WarrantyEndDate)]);
        }
    }
}
