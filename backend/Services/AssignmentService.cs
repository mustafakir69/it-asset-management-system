using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Services;

public enum AssignmentOperationStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record AssignmentOperationResult(
    AssignmentOperationStatus Status,
    AssignmentDto? Assignment = null,
    string? ErrorMessage = null);

public sealed class AssignmentService(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<AssignmentDto>> GetActiveAsync(
        string? search,
        string? department,
        CancellationToken cancellationToken)
    {
        var query = BuildReadQuery().Where(assignment => assignment.ReturnedAt == null);
        query = ApplyFilters(query, search, department);

        return await query
            .OrderByDescending(assignment => assignment.AssignedAt)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public Task<AssignmentDto?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        BuildReadQuery()
            .Where(assignment => assignment.Id == id)
            .Select(ToDtoExpression())
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<AssignmentDto>> GetHistoryAsync(
        string? search,
        string? department,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilters(BuildReadQuery(), search, department);
        if (isActive.HasValue)
        {
            query = isActive.Value
                ? query.Where(assignment => assignment.ReturnedAt == null)
                : query.Where(assignment => assignment.ReturnedAt != null);
        }

        return await query
            .OrderByDescending(assignment => assignment.AssignedAt)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssignmentDto>> GetMyActiveAsync(
        string employeeId,
        CancellationToken cancellationToken) =>
        await BuildReadQuery()
            .Where(assignment =>
                assignment.EmployeeId == employeeId && assignment.ReturnedAt == null)
            .OrderByDescending(assignment => assignment.AssignedAt)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);

    public async Task<AssignmentOperationResult> CreateAsync(
        AssignmentCreateDto request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var assetId = request.AssetId.Trim();
        var employeeId = request.EmployeeId.Trim();
        var asset = await dbContext.Assets
            .FirstOrDefaultAsync(item => item.Id == assetId, cancellationToken);
        if (asset is null)
        {
            return new(AssignmentOperationStatus.ValidationError, ErrorMessage: "Seçilen cihaz bulunamadı.");
        }

        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(item => item.Id == employeeId, cancellationToken);
        if (employee is null)
        {
            return new(AssignmentOperationStatus.ValidationError, ErrorMessage: "Seçilen çalışan bulunamadı.");
        }

        if (!employee.IsActive)
        {
            return new(AssignmentOperationStatus.ValidationError, ErrorMessage: "Pasif çalışana zimmet oluşturulamaz.");
        }

        if (!asset.Status.Equals("Stokta", StringComparison.OrdinalIgnoreCase))
        {
            return new(
                AssignmentOperationStatus.Conflict,
                ErrorMessage: $"{asset.Status} durumundaki cihaz zimmetlenemez. Yalnızca stoktaki cihazlar zimmetlenebilir.");
        }

        var hasActiveAssignment = await dbContext.Assignments
            .AnyAsync(
                assignment => assignment.AssetId == asset.Id && assignment.ReturnedAt == null,
                cancellationToken);
        if (hasActiveAssignment)
        {
            return new(AssignmentOperationStatus.Conflict, ErrorMessage: "Bu cihazın zaten aktif bir zimmeti var.");
        }

        var assignment = new Assignment
        {
            Id = Guid.NewGuid().ToString("N"),
            AssetId = asset.Id,
            EmployeeId = employee.Id,
            AssignedAt = request.AssignedAt!.Value,
            AssignedBy = request.AssignedBy.Trim(),
            Notes = Clean(request.Notes),
            CreatedAt = DateTimeOffset.UtcNow,
            Asset = asset,
            Employee = employee
        };

        asset.Status = "Zimmetli";
        dbContext.Assignments.Add(assignment);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(AssignmentOperationStatus.Success, ToDto(assignment));
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(AssignmentOperationStatus.Conflict, ErrorMessage: "Bu cihazın zaten aktif bir zimmeti var.");
        }
    }

    public async Task<AssignmentOperationResult> ReturnAsync(
        string id,
        AssignmentReturnDto request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var assignment = await dbContext.Assignments
            .Include(item => item.Asset)
            .Include(item => item.Employee)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (assignment is null)
        {
            return new(AssignmentOperationStatus.NotFound, ErrorMessage: "Zimmet kaydı bulunamadı.");
        }

        if (assignment.ReturnedAt.HasValue)
        {
            return new(AssignmentOperationStatus.Conflict, ErrorMessage: "Bu zimmet daha önce iade alınmış.");
        }

        if (request.ReturnedAt!.Value < assignment.AssignedAt)
        {
            return new(
                AssignmentOperationStatus.ValidationError,
                ErrorMessage: "İade tarihi zimmet tarihinden önce olamaz.");
        }

        assignment.ReturnedAt = request.ReturnedAt.Value;
        assignment.ReturnedBy = request.ReturnedBy.Trim();
        assignment.ReturnNotes = Clean(request.ReturnNotes);
        assignment.Asset.Status = "Stokta";

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(AssignmentOperationStatus.Success, ToDto(assignment));
    }

    private IQueryable<Assignment> BuildReadQuery() => dbContext.Assignments
        .AsNoTracking()
        .Include(assignment => assignment.Asset)
        .Include(assignment => assignment.Employee);

    private static IQueryable<Assignment> ApplyFilters(
        IQueryable<Assignment> query,
        string? search,
        string? department)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(assignment =>
                assignment.Asset.AssetCode.Contains(normalizedSearch) ||
                assignment.Employee.EmployeeNo.Contains(normalizedSearch) ||
                assignment.Employee.FullName.Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            var normalizedDepartment = department.Trim();
            query = query.Where(assignment => assignment.Employee.Department == normalizedDepartment);
        }

        return query;
    }

    private static System.Linq.Expressions.Expression<Func<Assignment, AssignmentDto>> ToDtoExpression() =>
        assignment => new AssignmentDto(
            assignment.Id,
            assignment.AssetId,
            assignment.Asset.AssetCode,
            assignment.Asset.Brand + " " + assignment.Asset.Model,
            assignment.Asset.Category,
            assignment.Asset.Brand,
            assignment.Asset.Model,
            assignment.Asset.Status,
            assignment.EmployeeId,
            assignment.Employee.EmployeeNo,
            assignment.Employee.FullName,
            assignment.Employee.Department,
            assignment.AssignedAt,
            assignment.ReturnedAt,
            assignment.AssignedBy,
            assignment.ReturnedBy,
            assignment.Notes,
            assignment.ReturnNotes,
            assignment.ReturnedAt == null);

    private static AssignmentDto ToDto(Assignment assignment) => new(
        assignment.Id,
        assignment.AssetId,
        assignment.Asset.AssetCode,
        $"{assignment.Asset.Brand} {assignment.Asset.Model}",
        assignment.Asset.Category,
        assignment.Asset.Brand,
        assignment.Asset.Model,
        assignment.Asset.Status,
        assignment.EmployeeId,
        assignment.Employee.EmployeeNo,
        assignment.Employee.FullName,
        assignment.Employee.Department,
        assignment.AssignedAt,
        assignment.ReturnedAt,
        assignment.AssignedBy,
        assignment.ReturnedBy,
        assignment.Notes,
        assignment.ReturnNotes,
        assignment.ReturnedAt is null);

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
