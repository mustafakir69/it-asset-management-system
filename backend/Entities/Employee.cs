namespace TakipProgrami.Api.Entities;

public sealed class Employee
{
    public string Id { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CorporateEmail { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public AppUser? AppUser { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = [];
    public ICollection<StockTransaction> ReceivedStockTransactions { get; set; } = [];
    public ICollection<MaintenanceRequest> SupportRequests { get; set; } = [];
}
