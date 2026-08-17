using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;

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
    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken) =>
        await dbContext.AppUsers
            .AsNoTracking()
            .Include(user => user.Employee)
            .OrderBy(user => user.Username)
            .Select(user => new UserDto(
                user.Id,
                user.EmployeeId,
                user.Employee == null ? null : user.Employee.FullName,
                user.Username,
                user.Email,
                user.Role.ToString(),
                GetRoleDisplayName(user.Role),
                user.IsActive))
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

        var username = request.Username.Trim();
        var email = request.Email.Trim();
        var employeeId = Clean(request.EmployeeId);

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

        if (await dbContext.AppUsers.AnyAsync(user => user.Username == username, cancellationToken))
        {
            return new(UserOperationStatus.Conflict, ErrorMessage: "Bu kullanıcı adı zaten kullanılıyor.");
        }

        if (await dbContext.AppUsers.AnyAsync(user => user.Email == email, cancellationToken))
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
            return new(UserOperationStatus.Success, ToDto(user));
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return new(UserOperationStatus.Conflict, ErrorMessage: "Kullanıcı adı, e-posta veya çalışan hesabı zaten kullanılıyor.");
        }
    }

    private static UserDto ToDto(AppUser user) => new(
        user.Id,
        user.EmployeeId,
        user.Employee?.FullName,
        user.Username,
        user.Email,
        user.Role.ToString(),
        GetRoleDisplayName(user.Role),
        user.IsActive);

    private static string GetRoleDisplayName(AppRole role) => role switch
    {
        AppRole.Admin => "Sistem Yöneticisi",
        AppRole.IT => "IT Yetkilisi",
        AppRole.Employee => "Çalışan",
        _ => role.ToString()
    };

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
