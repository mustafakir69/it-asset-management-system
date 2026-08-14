namespace TakipProgrami.Api.Entities;

public sealed class License
{
    public string Id { get; set; } = string.Empty;
    public string LicenseCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int UsedSeats { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
