using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed class StockItemCreateDto
{
    [Required(ErrorMessage = "Ürün kodu zorunludur.")]
    [MaxLength(50, ErrorMessage = "Ürün kodu en fazla 50 karakter olabilir.")]
    public string ItemCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [MaxLength(150, ErrorMessage = "Ürün adı en fazla 150 karakter olabilir.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Kategori zorunludur.")]
    [MaxLength(100, ErrorMessage = "Kategori en fazla 100 karakter olabilir.")]
    public string Category { get; init; } = string.Empty;

    [MaxLength(150, ErrorMessage = "Marka / model en fazla 150 karakter olabilir.")]
    public string BrandModel { get; init; } = string.Empty;

    [Required(ErrorMessage = "Birim zorunludur.")]
    [MaxLength(30, ErrorMessage = "Birim en fazla 30 karakter olabilir.")]
    public string Unit { get; init; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Başlangıç stok miktarı negatif olamaz.")]
    public int CurrentQuantity { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Minimum stok miktarı negatif olamaz.")]
    public int MinimumQuantity { get; init; }

    [Required(ErrorMessage = "Lokasyon zorunludur.")]
    [MaxLength(150, ErrorMessage = "Lokasyon en fazla 150 karakter olabilir.")]
    public string Location { get; init; } = string.Empty;
}
