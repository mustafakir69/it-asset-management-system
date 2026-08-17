using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Services;

public enum AssignmentOperationStatus { Success, NotFound, ValidationError, Conflict }
public sealed record AssignmentOperationResult(AssignmentOperationStatus Status, AssignmentDto? Assignment = null, string? ErrorMessage = null);

public sealed class AssignmentService(ApplicationDbContext dbContext)
{
    public Task<List<AssignmentDto>> GetActiveAsync(string? search, string? department, CancellationToken ct) =>
        Read(ApplyFilters(Query().Where(x => x.ReturnedAt == null), search, department), ct);

    public Task<List<AssignmentDto>> GetHistoryAsync(string? search, string? department, bool? active, CancellationToken ct)
    {
        var query = ApplyFilters(Query(), search, department);
        if (active.HasValue) query = query.Where(x => (x.ReturnedAt == null) == active.Value);
        return Read(query, ct);
    }

    public Task<AssignmentDto?> GetByIdAsync(string id, CancellationToken ct) =>
        Query().Where(x => x.Id == id).Select(ToDto()).FirstOrDefaultAsync(ct);

    public Task<List<AssignmentDto>> GetMyActiveAsync(string employeeId, CancellationToken ct) =>
        Read(Query().Where(x => x.EmployeeId == employeeId && x.ReturnedAt == null), ct);

    public async Task<AssignmentOperationResult> CreateAsync(AssignmentCreateDto request, string currentUserId, CancellationToken ct)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);
        var asset = await dbContext.Assets.FirstOrDefaultAsync(x => x.Id == request.AssetId, ct);
        var employee = await dbContext.Employees.FirstOrDefaultAsync(x => x.Id == request.EmployeeId, ct);
        if (asset is null) return Invalid("Seçilen cihaz bulunamadı.");
        if (employee is null || !employee.IsActive) return Invalid("Seçilen çalışan bulunamadı veya pasif.");
        if (asset.Status is "Hurda" or "Elden çıkarıldı") return Conflict("Hurda veya elden çıkarılmış cihaz zimmetlenemez.");
        if (!asset.Status.Equals("Stokta", StringComparison.OrdinalIgnoreCase)) return Conflict("Yalnızca stoktaki cihazlar zimmetlenebilir.");
        if (await dbContext.Assignments.AnyAsync(x => x.AssetId == asset.Id && x.ReturnedAt == null, ct)) return Conflict("Bu cihazın zaten aktif bir zimmeti var.");

        var assignment = new Assignment { Id = Guid.NewGuid().ToString("N"), AssetId = asset.Id, EmployeeId = employee.Id,
            AssignedAt = request.AssignedAt!.Value, AssignedByUserId = currentUserId, Notes = Clean(request.Notes),
            CreatedAt = DateTimeOffset.UtcNow, Asset = asset, Employee = employee };
        asset.Status = "Zimmetli";
        dbContext.Assignments.Add(assignment);
        try { await dbContext.SaveChangesAsync(ct); await tx.CommitAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 }) { await tx.RollbackAsync(ct); return Conflict("Bu cihazın zaten aktif bir zimmeti var."); }
        return new(AssignmentOperationStatus.Success, await GetByIdAsync(assignment.Id, ct));
    }

    public async Task<AssignmentOperationResult> ReturnAsync(string id, AssignmentReturnDto request, string currentUserId, CancellationToken ct)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);
        var assignment = await dbContext.Assignments.Include(x => x.Asset).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (assignment is null) return new(AssignmentOperationStatus.NotFound, ErrorMessage: "Zimmet kaydı bulunamadı.");
        if (assignment.ReturnedAt.HasValue) return Conflict("Bu zimmet daha önce iade alınmış.");
        if (request.ReturnedAt!.Value < assignment.AssignedAt) return Invalid("İade tarihi zimmet tarihinden önce olamaz.");
        assignment.ReturnedAt = request.ReturnedAt.Value; assignment.ReturnedByUserId = currentUserId;
        assignment.ReturnNotes = Clean(request.ReturnNotes); assignment.Asset.Status = "Stokta";
        await dbContext.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return new(AssignmentOperationStatus.Success, await GetByIdAsync(id, ct));
    }

    private IQueryable<Assignment> Query() => dbContext.Assignments.AsNoTracking();
    private static IQueryable<Assignment> ApplyFilters(IQueryable<Assignment> q, string? search, string? department)
    {
        if (!string.IsNullOrWhiteSpace(search)) { var s = search.Trim(); q = q.Where(x => x.Asset.AssetCode.Contains(s) || x.Employee.FullName.Contains(s) || x.Asset.Brand.Contains(s) || x.Asset.Model.Contains(s)); }
        if (!string.IsNullOrWhiteSpace(department)) { var d = department.Trim(); q = q.Where(x => x.Employee.Department == d); }
        return q;
    }
    private static Task<List<AssignmentDto>> Read(IQueryable<Assignment> q, CancellationToken ct) => q.OrderByDescending(x => x.AssignedAt).Select(ToDto()).ToListAsync(ct);
    private static System.Linq.Expressions.Expression<Func<Assignment, AssignmentDto>> ToDto() => x => new(
        x.Id, x.AssetId, x.Asset.AssetCode, x.Asset.Brand + " " + x.Asset.Model, x.Asset.Category, x.Asset.Brand, x.Asset.Model, x.Asset.Status,
        x.EmployeeId, x.Employee.EmployeeNo, x.Employee.FullName, x.Employee.Department, x.AssignedAt, x.ReturnedAt,
        x.AssignedByUserId, x.AssignedByUser.Employee != null ? x.AssignedByUser.Employee.FullName : x.AssignedByUser.Username,
        x.ReturnedByUserId, x.ReturnedByUser == null ? null : x.ReturnedByUser.Employee != null ? x.ReturnedByUser.Employee.FullName : x.ReturnedByUser.Username,
        x.Notes, x.ReturnNotes, x.ReturnedAt == null);
    private static AssignmentOperationResult Invalid(string error) => new(AssignmentOperationStatus.ValidationError, ErrorMessage: error);
    private static AssignmentOperationResult Conflict(string error) => new(AssignmentOperationStatus.Conflict, ErrorMessage: error);
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
