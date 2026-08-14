namespace TakipProgrami.Api.Entities;

public sealed class StockItem
{
    public string Id { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string BrandModel { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int MinimumQuantity { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public ICollection<StockTransaction> Transactions { get; set; } = [];
}
