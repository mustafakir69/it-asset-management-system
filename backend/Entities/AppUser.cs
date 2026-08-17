namespace TakipProgrami.Api.Entities;

public sealed class AppUser
{
    public string Id { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public AppRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public Employee? Employee { get; set; }
    public ICollection<Assignment> AssignmentsCreated { get; set; } = [];
    public ICollection<Assignment> AssignmentsReturned { get; set; } = [];
    public ICollection<StockTransaction> StockTransactions { get; set; } = [];
    public ICollection<MaintenancePlan> ResponsibleMaintenancePlans { get; set; } = [];
    public ICollection<MaintenanceTask> CompletedMaintenanceTasks { get; set; } = [];
    public ICollection<MaintenanceRequest> AssignedSupportRequests { get; set; } = [];
    public ICollection<MaintenanceRequest> CompletedSupportRequests { get; set; } = [];
}
