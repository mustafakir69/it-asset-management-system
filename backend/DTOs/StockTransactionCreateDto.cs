using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed class StockTransactionCreateDto
{
    [Required(ErrorMessage = "İşlem tipi zorunludur.")]
    public string TransactionType { get; init; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
    public int Quantity { get; init; }

    [Required(ErrorMessage = "İşlem tarihi zorunludur.")]
    public DateTimeOffset? TransactionDate { get; init; }

    [Required(ErrorMessage = "İşlemi yapan veya teslim alan kişi zorunludur.")]
    [MaxLength(150, ErrorMessage = "Kişi adı en fazla 150 karakter olabilir.")]
    public string PersonName { get; init; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir.")]
    public string? Note { get; init; }
}
