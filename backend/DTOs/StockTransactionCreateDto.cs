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

    public string? RecipientEmployeeId { get; init; }

    [MaxLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir.")]
    public string? Note { get; init; }
}
