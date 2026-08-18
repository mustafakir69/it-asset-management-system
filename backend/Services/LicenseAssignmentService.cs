using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Services;

public enum LicenseAssignmentOperationStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record LicenseAssignmentOperationResult(
    LicenseAssignmentOperationStatus Status,
    LicenseAssignmentDto? Assignment = null,
    string? ErrorMessage = null);

public sealed class LicenseAssignmentService(ApplicationDbContext db)
{
    public Task<List<LicenseAssignmentDto>> GetByLicenseAsync(
        string licenseId,
        CancellationToken ct) =>
        Query().Where(item => item.LicenseId == licenseId)
            .OrderByDescending(item => item.AssignedAt)
            .Select(ToDto())
            .ToListAsync(ct);

    public Task<List<LicenseAssignmentDto>> GetActiveByAssetAsync(
        string assetId,
        CancellationToken ct) =>
        Query().Where(item => item.AssetId == assetId && item.RevokedAt == null)
            .OrderByDescending(item => item.AssignedAt)
            .Select(ToDto())
            .ToListAsync(ct);

    public async Task<LicenseAssignmentOperationResult> CreateAsync(
        string licenseId,
        LicenseAssignmentCreateDto input,
        string currentUserId,
        CancellationToken ct)
    {
        var employeeId = Clean(input.EmployeeId);
        var assetId = Clean(input.AssetId);
        if (employeeId is null && assetId is null)
            return Invalid("En az bir çalışan veya cihaz seçilmelidir.");

        await using var transaction = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            ct);
        var license = await db.Licenses.FirstOrDefaultAsync(item => item.Id == licenseId, ct);
        if (license is null) return NotFound("Lisans bulunamadı.");
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (!license.IsActive) return Conflict("Pasif lisansa yeni atama yapılamaz.");
        if (license.ExpirationDate is { } expirationDate && expirationDate < today)
            return Conflict("Süresi dolmuş lisansa yeni atama yapılamaz.");

        if (employeeId is not null && !await db.Employees.AnyAsync(
                employee => employee.Id == employeeId && employee.IsActive,
                ct))
            return Invalid("Seçilen çalışan bulunamadı veya pasif.");
        if (assetId is not null && !await db.Assets.AnyAsync(asset => asset.Id == assetId, ct))
            return Invalid("Seçilen cihaz bulunamadı.");

        if (await db.LicenseAssignments.AnyAsync(
                item => item.LicenseId == licenseId && item.EmployeeId == employeeId &&
                    item.AssetId == assetId && item.RevokedAt == null,
                ct))
            return Conflict("Bu lisans aynı çalışan/cihaz kombinasyonuna zaten aktif olarak atanmış.");

        var activeCount = await db.LicenseAssignments.CountAsync(
            item => item.LicenseId == licenseId && item.RevokedAt == null,
            ct);
        if (activeCount >= license.TotalSeats)
            return Conflict("Aktif atama sayısı toplam lisans hakkını aşamaz.");

        var assignment = new LicenseAssignment
        {
            Id = Guid.NewGuid().ToString("N"),
            LicenseId = licenseId,
            EmployeeId = employeeId,
            AssetId = assetId,
            AssignedAt = input.AssignedAt!.Value,
            AssignedByUserId = currentUserId
        };
        db.LicenseAssignments.Add(assignment);
        try
        {
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            await transaction.RollbackAsync(ct);
            return Conflict("Bu lisans aynı çalışan/cihaz kombinasyonuna zaten aktif olarak atanmış.");
        }

        return new(
            LicenseAssignmentOperationStatus.Success,
            await Query().Where(item => item.Id == assignment.Id).Select(ToDto()).FirstAsync(ct));
    }

    public async Task<LicenseAssignmentOperationResult> RevokeAsync(
        string licenseId,
        string assignmentId,
        string currentUserId,
        CancellationToken ct)
    {
        var assignment = await db.LicenseAssignments.FirstOrDefaultAsync(
            item => item.Id == assignmentId && item.LicenseId == licenseId,
            ct);
        if (assignment is null) return NotFound("Lisans ataması bulunamadı.");
        if (assignment.RevokedAt is not null) return Conflict("Bu lisans ataması daha önce kaldırılmış.");

        assignment.RevokedAt = DateTimeOffset.UtcNow;
        assignment.RevokedByUserId = currentUserId;
        await db.SaveChangesAsync(ct);
        return new(
            LicenseAssignmentOperationStatus.Success,
            await Query().Where(item => item.Id == assignment.Id).Select(ToDto()).FirstAsync(ct));
    }

    private IQueryable<LicenseAssignment> Query() => db.LicenseAssignments.AsNoTracking();

    private static System.Linq.Expressions.Expression<Func<LicenseAssignment, LicenseAssignmentDto>> ToDto() =>
        item => new(
            item.Id,
            item.LicenseId,
            item.License.LicenseCode,
            item.License.ProductName,
            item.License.LicenseType,
            item.EmployeeId,
            item.Employee == null ? null : item.Employee.FullName,
            item.Employee == null ? null : item.Employee.Department,
            item.AssetId,
            item.Asset == null ? null : item.Asset.AssetCode,
            item.Asset == null ? null : item.Asset.Brand + " " + item.Asset.Model,
            item.AssignedAt,
            item.AssignedByUserId,
            item.AssignedByUser.Employee != null
                ? item.AssignedByUser.Employee.FullName
                : item.AssignedByUser.Username,
            item.RevokedAt,
            item.RevokedByUserId,
            item.RevokedByUser == null
                ? null
                : item.RevokedByUser.Employee != null
                    ? item.RevokedByUser.Employee.FullName
                    : item.RevokedByUser.Username,
            item.RevokedAt == null ? "Aktif" : "Kaldırıldı");

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static LicenseAssignmentOperationResult Invalid(string message) =>
        new(LicenseAssignmentOperationStatus.ValidationError, ErrorMessage: message);
    private static LicenseAssignmentOperationResult Conflict(string message) =>
        new(LicenseAssignmentOperationStatus.Conflict, ErrorMessage: message);
    private static LicenseAssignmentOperationResult NotFound(string message) =>
        new(LicenseAssignmentOperationStatus.NotFound, ErrorMessage: message);
}
