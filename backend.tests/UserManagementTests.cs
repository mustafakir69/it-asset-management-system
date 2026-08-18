using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TakipProgrami.Api.Controllers;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Tests;

public sealed class UserManagementTests
{
    [Fact]
    public async Task Admin_CanDeactivateReactivateAndResetEmployeePassword()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var employee = CoreBusinessRulesTests.Employee("managed-employee");
        var user = CoreBusinessRulesTests.User("managed-user", AppRole.Employee);
        user.EmployeeId = employee.Id;
        const string oldPassword = "OldPassword-123!";
        const string newPassword = "NewPassword-456!";
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, oldPassword);
        db.AddRange(employee, user);
        await db.SaveChangesAsync();
        var service = new UserService(db, hasher);

        var deactivated = await service.SetActiveAsync(
            user.Id, false, AppRole.Admin, "another-admin", CancellationToken.None);

        Assert.Equal(UserOperationStatus.Success, deactivated.Status);
        Assert.False(user.IsActive);
        var loginController = new AuthController(db, hasher, JwtService());
        var inactiveLogin = await loginController.Login(
            new LoginRequestDto(user.Username, oldPassword), CancellationToken.None);
        Assert.Equal(403, Assert.IsType<ObjectResult>(inactiveLogin.Result).StatusCode);

        var reactivated = await service.SetActiveAsync(
            user.Id, true, AppRole.Admin, "another-admin", CancellationToken.None);
        var reset = await service.ResetPasswordAsync(
            user.Id, newPassword, AppRole.Admin, CancellationToken.None);

        Assert.Equal(UserOperationStatus.Success, reactivated.Status);
        Assert.Equal(UserOperationStatus.Success, reset.Status);
        Assert.True(user.IsActive);
        Assert.NotEqual(newPassword, user.PasswordHash);
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(user, user.PasswordHash, newPassword));
        Assert.Equal(
            PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(user, user.PasswordHash, oldPassword));
    }

    [Fact]
    public async Task It_CanManageEmployeeButCannotManageItOrAdminAccounts()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var employee = CoreBusinessRulesTests.Employee("employee-target");
        var employeeUser = CoreBusinessRulesTests.User("employee-target-user", AppRole.Employee);
        employeeUser.EmployeeId = employee.Id;
        var itUser = CoreBusinessRulesTests.User("it-target-user", AppRole.IT);
        var adminUser = CoreBusinessRulesTests.User("admin-target-user", AppRole.Admin);
        db.AddRange(employee, employeeUser, itUser, adminUser);
        await db.SaveChangesAsync();
        var service = new UserService(db, new PasswordHasher<AppUser>());

        var employeeResult = await service.SetActiveAsync(
            employeeUser.Id, false, AppRole.IT, "caller-it", CancellationToken.None);
        var itResult = await service.SetActiveAsync(
            itUser.Id, false, AppRole.IT, "caller-it", CancellationToken.None);
        var adminResult = await service.ResetPasswordAsync(
            adminUser.Id, "NewPassword-456!", AppRole.IT, CancellationToken.None);

        Assert.Equal(UserOperationStatus.Success, employeeResult.Status);
        Assert.Equal(UserOperationStatus.Forbidden, itResult.Status);
        Assert.Equal(UserOperationStatus.Forbidden, adminResult.Status);
    }

    [Fact]
    public async Task BootstrapAdminAndCurrentUserCannotBeDeactivated()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var bootstrap = CoreBusinessRulesTests.User("bootstrap-admin", AppRole.Admin);
        var employee = CoreBusinessRulesTests.Employee("self-employee");
        var self = CoreBusinessRulesTests.User("self-user", AppRole.Employee);
        self.EmployeeId = employee.Id;
        db.AddRange(bootstrap, employee, self);
        await db.SaveChangesAsync();
        var service = new UserService(db, new PasswordHasher<AppUser>());

        var bootstrapResult = await service.SetActiveAsync(
            bootstrap.Id, false, AppRole.Admin, "another-admin", CancellationToken.None);
        var selfResult = await service.SetActiveAsync(
            self.Id, false, AppRole.Admin, self.Id, CancellationToken.None);

        Assert.Equal(UserOperationStatus.Conflict, bootstrapResult.Status);
        Assert.Equal(UserOperationStatus.Conflict, selfResult.Status);
        Assert.True(bootstrap.IsActive);
        Assert.True(self.IsActive);
    }

    [Fact]
    public async Task Update_UsesRelationalAccountRulesAndRejectsDuplicateUsername()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var employee = CoreBusinessRulesTests.Employee("update-employee");
        var user = CoreBusinessRulesTests.User("update-user", AppRole.Employee);
        user.EmployeeId = employee.Id;
        var existing = CoreBusinessRulesTests.User("existing-user", AppRole.Employee);
        existing.Username = "existing.username";
        db.AddRange(employee, user, existing);
        await db.SaveChangesAsync();
        var service = new UserService(db, new PasswordHasher<AppUser>());

        var duplicate = await service.UpdateAsync(user.Id, new UserUpdateDto
        {
            Username = "EXISTING.USERNAME",
            Email = user.Email,
            Role = "Employee"
        }, AppRole.Admin, CancellationToken.None);
        var invalidAdminPromotion = await service.UpdateAsync(user.Id, new UserUpdateDto
        {
            Username = user.Username,
            Email = user.Email,
            Role = "Admin"
        }, AppRole.Admin, CancellationToken.None);

        Assert.Equal(UserOperationStatus.Conflict, duplicate.Status);
        Assert.Equal(UserOperationStatus.ValidationError, invalidAdminPromotion.Status);
        Assert.Equal(AppRole.Employee, user.Role);
    }

    private static JwtTokenService JwtService() => new(Options.Create(new JwtOptions
    {
        Key = "test-only-key-with-at-least-32-characters-123456",
        Issuer = "tests",
        Audience = "tests",
        ExpirationMinutes = 10
    }));
}
