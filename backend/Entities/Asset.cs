namespace TakipProgrami.Api.Entities;

public sealed class Asset
{
    public string Id { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateOnly PurchaseDate { get; set; }
    public DateOnly WarrantyEndDate { get; set; }
    public ICollection<MaintenancePlan> MaintenancePlans { get; set; } = [];
    public ICollection<MaintenanceTask> MaintenanceTasks { get; set; } = [];
    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = [];
}
