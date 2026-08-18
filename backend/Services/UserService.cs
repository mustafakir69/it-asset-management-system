using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Services;

public enum UserOperationStatus
{
    Success,
    ValidationError,
    Conflict,
    Forbidden
}

public sealed record UserOperationResult(
    UserOperationStatus Status,
    UserDto? User = null,
    string? ErrorMessage = null);

public sealed class UserService(
    ApplicationDbContext dbContext,
    IPasswordHasher<AppUser> passwordHasher)
{
    public async Task<string?> SuggestUsernameAsync(
        string employeeId,
        CancellationToken cancellationToken)
    {
        var fullName = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.Id == employeeId && employee.IsActive)
            .Select(employee => employee.FullName)
            .FirstOrDefaultAsync(cancellationToken);
        var baseUsername = UsernameRules.FromFullName(fullName);
        return baseUsername is null
            ? null
            : await FindAvailableUsernameAsync(baseUsername, cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken) =>
        await dbContext.AppUsers
            .AsNoTracking()
            .Include(user => user.Employee)
            .OrderBy(user => user.Username)
            .Select(user => new UserDto(
                user.Id,
                user.EmployeeId,
                user.Employee == null || user.Employee.FullName.Trim() == ""
                    ? user.Username
                    : user.Employee.FullName,
                user.Employee == null || user.Employee.Department.Trim() == ""
                    ? null
                    : user.Employee.Department,
                user.Employee == null || user.Employee.EmployeeNo.Trim() == ""
                    ? null
                    : user.Employee.EmployeeNo,
                user.Username,
                user.Email,
                user.Role.ToString(),
                GetRoleDisplayName(user.Role),
                user.IsActive,
                user.IsActive ? "Aktif" : "Pasif"))
            .ToListAsync(cancellationToken);

    public async Task<UserOperationResult> CreateAsync(
        UserCreateDto request,
        AppRole callerRole,
        CancellationToken cancellationToken)
    {
        if (request.Role.Trim().Equals("Auditor", StringComparison.OrdinalIgnoreCase) ||
            !Enum.TryParse<AppRole>(request.Role.Trim(), true, out var requestedRole))
        {
            return new(UserOperationStatus.ValidationError, ErrorMessage: "Geçerli bir kullanıcı rolü seçin.");
        }

        if (callerRole == AppRole.IT && requestedRole != AppRole.Employee)
        {
            return new(UserOperationStatus.Forbidden, ErrorMessage: "IT kullanıcıları yalnızca Çalışan rolünde hesap oluşturabilir.");
        }

        var requestedUsername = Clean(request.Username);
        var email = Clean(request.Email)?.ToLowerInvariant();
        var employeeId = Clean(request.EmployeeId);

        if (email is null)
        {
            return new(UserOperationStatus.ValidationError, ErrorMessage: "E-posta adresi zorunludur.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        if (requestedRole is AppRole.Employee or AppRole.IT && employeeId is null)
        {
            return new(UserOperationStatus.ValidationError, ErrorMessage: "Çalışan rolü için çalışan seçimi zorunludur.");
        }

        if (requestedRole == AppRole.Admin && employeeId is not null)
        {
            return new(UserOperationStatus.ValidationError, ErrorMessage: "Sistem yöneticisi hesabı bir çalışanla ilişkilendirilemez.");
        }

        Employee? employee = null;
        if (employeeId is not null)
        {
            employee = await dbContext.Employees
                .FirstOrDefaultAsync(item => item.Id == employeeId, cancellationToken);
            if (employee is null)
            {
                return new(UserOperationStatus.ValidationError, ErrorMessage: "Seçilen çalışan bulunamadı.");
            }

            if (!employee.IsActive)
            {
                return new(UserOperationStatus.ValidationError, ErrorMessage: "Pasif çalışan için kullanıcı hesabı oluşturulamaz.");
            }

            if (await dbContext.AppUsers.AnyAsync(user => user.EmployeeId == employeeId, cancellationToken))
            {
                return new(UserOperationStatus.Conflict, ErrorMessage: "Seçilen çalışanın zaten bir kullanıcı hesabı var.");
            }
        }

        var username = requestedUsername is null
            ? UsernameRules.FromFullName(employee?.FullName)
            : UsernameRules.Normalize(requestedUsername);
        if (username is null || username.Length < 3)
        {
            return new(
                UserOperationStatus.ValidationError,
                ErrorMessage: requestedUsername is null
                    ? "Ad Soyad bilgisi kullanıcı adı oluşturmak için uygun değil."
                    : "Kullanıcı adı oluşturulamadı.");
        }

        if (requestedUsername is null)
        {
            username = await FindAvailableUsernameAsync(username, cancellationToken);
        }

        if (await dbContext.AppUsers.AnyAsync(
                user => user.Username.ToLower() == username.ToLower(),
                cancellationToken))
        {
            return new(UserOperationStatus.Conflict, ErrorMessage: "Bu kullanıcı adı zaten kullanılıyor.");
        }

        if (await dbContext.AppUsers.AnyAsync(
                user => user.Email.ToLower() == email.ToLower(),
                cancellationToken))
        {
            return new(UserOperationStatus.Conflict, ErrorMessage: "Bu e-posta adresi zaten kullanılıyor.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString("N"),
            EmployeeId = employeeId,
            Username = username,
            Email = email,
            Role = requestedRole,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            Employee = employee
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        dbContext.AppUsers.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(UserOperationStatus.Success, ToDto(user));
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return new(UserOperationStatus.Conflict, ErrorMessage: GetUniqueViolationMessage(exception));
        }
    }

    private async Task<string> FindAvailableUsernameAsync(
        string baseUsername,
        CancellationToken cancellationToken)
    {
        var existingUsernames = await dbContext.AppUsers
            .AsNoTracking()
            .Select(user => user.Username)
            .ToListAsync(cancellationToken);
        var used = existingUsernames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return UsernameRules.FirstAvailable(baseUsername, used);
    }

    private static UserDto ToDto(AppUser user) => new(
        user.Id,
        user.EmployeeId,
        DisplayName(user.Employee?.FullName, user.Username),
        Clean(user.Employee?.Department),
        Clean(user.Employee?.EmployeeNo),
        user.Username,
        user.Email,
        user.Role.ToString(),
        GetRoleDisplayName(user.Role),
        user.IsActive,
        user.IsActive ? "Aktif" : "Pasif");

    private static string GetRoleDisplayName(AppRole role) => role switch
    {
        AppRole.Admin => "Yönetici",
        AppRole.IT => "IT Yetkilisi",
        AppRole.Employee => "Çalışan",
        _ => role.ToString()
    };

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string DisplayName(string? fullName, string username) =>
        string.IsNullOrWhiteSpace(fullName) ? username : fullName.Trim();

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private static string GetUniqueViolationMessage(DbUpdateException exception)
    {
        var detail = exception.InnerException?.Message ?? exception.Message;
        if (detail.Contains("UX_AppUsers_Username", StringComparison.OrdinalIgnoreCase))
            return "Bu kullanıcı adı zaten kullanılıyor.";
        if (detail.Contains("UX_AppUsers_Email", StringComparison.OrdinalIgnoreCase))
            return "Bu e-posta adresi zaten kullanılıyor.";
        if (detail.Contains("UX_AppUsers_EmployeeId", StringComparison.OrdinalIgnoreCase))
            return "Seçilen çalışanın zaten bir kullanıcı hesabı var.";
        return "Kullanıcı adı, e-posta veya çalışan hesabı zaten kullanılıyor.";
    }
}
