namespace TakipProgrami.Api.DTOs;

public sealed record LicenseDto(
    string Id,
    string LicenseCode,
    string ProductName,
    string Vendor,
    string LicenseType,
    int TotalSeats,
    int UsedSeats,
    int AvailableSeats,
    DateOnly StartDate,
    DateOnly? ExpirationDate,
    bool IsActive,
    string? Notes,
    string LicenseStatus);
