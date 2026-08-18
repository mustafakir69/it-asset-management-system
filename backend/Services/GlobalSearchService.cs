using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Services;

public sealed class GlobalSearchService(ApplicationDbContext db)
{
    public async Task<IReadOnlyList<GlobalSearchResultDto>> SearchAsync(
        string query,
        int limitPerCategory,
        bool isEmployee,
        string? employeeId,
        CancellationToken cancellationToken)
    {
        var term = query.Trim();
        if (isEmployee)
            return employeeId is null
                ? []
                : await SearchEmployeeScopeAsync(term, employeeId, limitPerCategory, cancellationToken);

        var userValues = await db.AppUsers.AsNoTracking()
            .Where(user =>
                user.Username.Contains(term) ||
                user.Email.Contains(term) ||
                (user.Employee != null &&
                    (user.Employee.FullName.Contains(term) ||
                     user.Employee.Department.Contains(term))))
            .OrderBy(user => user.Employee == null ? user.Username : user.Employee.FullName)
            .Take(limitPerCategory)
            .Select(user => new
            {
                user.Username,
                user.Role,
                FullName = user.Employee == null ? user.Username : user.Employee.FullName,
                Department = user.Employee == null ? null : user.Employee.Department
            })
            .ToListAsync(cancellationToken);
        var users = userValues.Select(user => new GlobalSearchResultDto(
            "Kullanıcılar",
            user.FullName,
            user.Username + " · " + (user.Department ?? user.Role.ToString()),
            "/admin/users?search=" + Uri.EscapeDataString(user.Username)));

        var assets = await db.Assets.AsNoTracking()
            .Where(asset =>
                asset.AssetCode.Contains(term) ||
                asset.SerialNumber.Contains(term) ||
                asset.Brand.Contains(term) ||
                asset.Model.Contains(term) ||
                asset.Assignments.Any(assignment =>
                    assignment.ReturnedAt == null && assignment.Employee.FullName.Contains(term)))
            .OrderBy(asset => asset.AssetCode)
            .Take(limitPerCategory)
            .Select(asset => new GlobalSearchResultDto(
                "Envanter",
                asset.AssetCode,
                asset.Brand + " " + asset.Model + " · " + asset.Status,
                "/assets/" + asset.Id))
            .ToListAsync(cancellationToken);

        var licenses = await db.Licenses.AsNoTracking()
            .Where(license =>
                license.LicenseCode.Contains(term) ||
                license.ProductName.Contains(term) ||
                license.Vendor.Contains(term))
            .OrderBy(license => license.LicenseCode)
            .Take(limitPerCategory)
            .Select(license => new GlobalSearchResultDto(
                "Lisanslar",
                license.LicenseCode,
                license.ProductName + " · " + license.Vendor,
                "/licenses/" + license.Id))
            .ToListAsync(cancellationToken);

        var maintenance = await db.MaintenanceTasks.AsNoTracking()
            .Where(task =>
                task.Title.Contains(term) ||
                task.Asset.AssetCode.Contains(term) ||
                task.Asset.Brand.Contains(term) ||
                task.Asset.Model.Contains(term) ||
                task.MaintenancePlan.ResponsibleUser.Username.Contains(term) ||
                (task.MaintenancePlan.ResponsibleUser.Employee != null &&
                    task.MaintenancePlan.ResponsibleUser.Employee.FullName.Contains(term)))
            .OrderByDescending(task => task.PlannedDate)
            .Take(limitPerCategory)
            .Select(task => new GlobalSearchResultDto(
                "Periyodik Bakım",
                task.Title,
                task.Asset.AssetCode + " · " + task.Asset.Brand + " " + task.Asset.Model,
                "/maintenance/tasks/" + task.Id))
            .ToListAsync(cancellationToken);

        var support = await SearchSupportAsync(term, null, limitPerCategory, cancellationToken);
        return users.Concat(assets).Concat(licenses).Concat(maintenance).Concat(support).ToList();
    }

    private async Task<IReadOnlyList<GlobalSearchResultDto>> SearchEmployeeScopeAsync(
        string term,
        string employeeId,
        int limitPerCategory,
        CancellationToken cancellationToken)
    {
        var assignments = await db.Assignments.AsNoTracking()
            .Where(assignment =>
                assignment.EmployeeId == employeeId &&
                assignment.ReturnedAt == null &&
                (assignment.Asset.AssetCode.Contains(term) ||
                 assignment.Asset.SerialNumber.Contains(term) ||
                 assignment.Asset.Brand.Contains(term) ||
                 assignment.Asset.Model.Contains(term)))
            .OrderBy(assignment => assignment.Asset.AssetCode)
            .Take(limitPerCategory)
            .Select(assignment => new GlobalSearchResultDto(
                "Zimmetlerim",
                assignment.Asset.AssetCode,
                assignment.Asset.Brand + " " + assignment.Asset.Model,
                "/assignments/" + assignment.Id))
            .ToListAsync(cancellationToken);

        var support = await SearchSupportAsync(term, employeeId, limitPerCategory, cancellationToken);
        return assignments.Concat(support).ToList();
    }

    private async Task<List<GlobalSearchResultDto>> SearchSupportAsync(
        string term,
        string? employeeId,
        int limitPerCategory,
        CancellationToken cancellationToken)
    {
        var idTerm = term.StartsWith("BT-", StringComparison.OrdinalIgnoreCase)
            ? term[3..]
            : term;
        idTerm = idTerm.ToLowerInvariant();
        var query = db.MaintenanceRequests.AsNoTracking().AsQueryable();
        if (employeeId is not null)
            query = query.Where(request => request.RequestedByEmployeeId == employeeId);

        return await query
            .Where(request =>
                request.Title.Contains(term) ||
                request.Asset.AssetCode.Contains(term) ||
                request.RequestedByEmployee.FullName.Contains(term) ||
                request.Id.ToLower().StartsWith(idTerm))
            .OrderByDescending(request => request.UpdatedAt)
            .Take(limitPerCategory)
            .Select(request => new GlobalSearchResultDto(
                "Teknik Destek",
                "BT-" + request.Id.Substring(0, 8).ToUpper(),
                request.Title + " · " + request.Asset.AssetCode,
                "/support-requests/" + request.Id))
            .ToListAsync(cancellationToken);
    }
}
