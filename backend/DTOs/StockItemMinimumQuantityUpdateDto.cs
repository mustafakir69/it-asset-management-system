using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed class StockItemMinimumQuantityUpdateDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Minimum stok miktarı negatif olamaz.")]
    public int MinimumQuantity { get; init; }
}
