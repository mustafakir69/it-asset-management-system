using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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

public sealed class CoreBusinessRulesTests
{
    [Fact]
    public async Task AssignmentActors_AreTakenFromCurrentUser()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var asset = Asset("actor-asset", "Stokta");
        var employee = Employee("actor-employee");
        var user = User("actor-it", AppRole.IT);
        dbContext.AddRange(asset, employee, user);
        await dbContext.SaveChangesAsync();
        var service = new AssignmentService(dbContext);

        var created = await service.CreateAsync(new AssignmentCreateDto
        {
            AssetId = asset.Id, EmployeeId = employee.Id,
            AssignedAt = DateTimeOffset.UtcNow.AddHours(-1)
        }, user.Id, CancellationToken.None);
        var assignment = await dbContext.Assignments.SingleAsync();
        Assert.Equal(AssignmentOperationStatus.Success, created.Status);
        Assert.Equal(user.Id, assignment.AssignedByUserId);
        Assert.Equal("Zimmetli", asset.Status);

        var returned = await service.ReturnAsync(assignment.Id, new AssignmentReturnDto
        {
            ReturnedAt = DateTimeOffset.UtcNow
        }, user.Id, CancellationToken.None);
        Assert.Equal(AssignmentOperationStatus.Success, returned.Status);
        Assert.Equal(user.Id, assignment.ReturnedByUserId);
        Assert.Equal("Stokta", asset.Status);
    }

    [Fact]
    public async Task StockTransaction_UsesCurrentUserAndRealRecipient()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var item = StockItem("actor-stock", 5, 1); var employee = Employee("stock-recipient"); var user = User("stock-it", AppRole.IT);
        dbContext.AddRange(item, employee, user); await dbContext.SaveChangesAsync();
        var result = await new StockService(dbContext, TestInfrastructure.CreateNotificationService(dbContext)).CreateTransactionAsync(
            item.Id, new StockTransactionCreateDto { TransactionType = "Çıkış", Quantity = 1, TransactionDate = DateTimeOffset.UtcNow, RecipientEmployeeId = employee.Id }, user.Id, CancellationToken.None);
        Assert.Equal(StockTransactionResultStatus.Success, result.Status);
        var transaction = await dbContext.StockTransactions.SingleAsync();
        Assert.Equal(user.Id, transaction.PerformedByUserId);
        Assert.Equal(employee.Id, transaction.RecipientEmployeeId);
    }

    [Fact]
    public async Task UserService_RejectsAuditorRole()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var service = new UserService(dbContext, new PasswordHasher<AppUser>());
        var result = await service.CreateAsync(new UserCreateDto
        {
            Username = "auditor.test", Email = "auditor@example.test", Password = "TestOnly-123!", Role = "Auditor"
        }, AppRole.Admin, CancellationToken.None);
        Assert.Equal(UserOperationStatus.ValidationError, result.Status);
        Assert.Empty(dbContext.AppUsers);
    }

    [Fact]
    public async Task SupportRequest_UsesEmployeeIdFromJwt()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var asset = Asset("support-owned", "Zimmetli");
        var employee = Employee("support-owner");
        var actor = User("support-user", AppRole.Employee);
        actor.EmployeeId = employee.Id;
        dbContext.AddRange(asset, employee, actor, new Assignment
        {
            Id = "support-assignment", AssetId = asset.Id, EmployeeId = employee.Id,
            AssignedAt = DateTimeOffset.UtcNow.AddDays(-2), AssignedByUserId = actor.Id,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
        });
        await dbContext.SaveChangesAsync();
        var controller = WithUser(
            new MaintenanceRequestsController(dbContext),
            actor.Id,
            AppRole.Employee,
            employee.Id);

        var result = await controller.Create(new MaintenanceRequestCreateDto
        {
            AssetId = asset.Id,
            Title = "VPN bağlantısı",
            Description = "Kurumsal VPN bağlantısı kurulamıyor.",
            Priority = "Yüksek"
        }, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result.Result);
        var request = await dbContext.MaintenanceRequests.SingleAsync();
        Assert.Equal(employee.Id, request.RequestedByEmployeeId);
    }

    [Fact]
    public async Task SupportRequest_RejectsAnotherEmployeesAsset()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var asset = Asset("support-foreign", "Zimmetli");
        var owner = Employee("support-owner-2");
        var requester = Employee("support-requester");
        var itUser = User("support-it", AppRole.IT);
        var employeeUser = User("support-requester-user", AppRole.Employee);
        employeeUser.EmployeeId = requester.Id;
        dbContext.AddRange(asset, owner, requester, itUser, employeeUser, new Assignment
        {
            Id = "foreign-assignment", AssetId = asset.Id, EmployeeId = owner.Id,
            AssignedAt = DateTimeOffset.UtcNow.AddDays(-1), AssignedByUserId = itUser.Id,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        });
        await dbContext.SaveChangesAsync();
        var controller = WithUser(
            new MaintenanceRequestsController(dbContext),
            employeeUser.Id,
            AppRole.Employee,
            requester.Id);

        var result = await controller.Create(new MaintenanceRequestCreateDto
        {
            AssetId = asset.Id,
            Title = "Yetkisiz cihaz",
            Description = "Başka çalışanın cihazı için talep.",
            Priority = "Normal"
        }, CancellationToken.None);

        var forbidden = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        Assert.Empty(dbContext.MaintenanceRequests);
    }

    [Fact]
    public async Task SupportAssignment_RejectsNonItUser()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var asset = Asset("support-assign", "Zimmetli");
        var employee = Employee("support-assign-owner");
        var nonItUser = User("support-non-it", AppRole.Employee);
        nonItUser.EmployeeId = employee.Id;
        dbContext.AddRange(asset, employee, nonItUser, new MaintenanceRequest
        {
            Id = "support-to-assign", AssetId = asset.Id, RequestedByEmployeeId = employee.Id,
            Title = "Destek", Description = "Test destek talebi.",
            Priority = MaintenanceRequestPriority.Normal, Status = MaintenanceRequestStatus.Open,
            CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
        var controller = WithUser(
            new MaintenanceRequestsController(dbContext),
            "admin-user",
            AppRole.Admin);

        var result = await controller.Assign(
            "support-to-assign",
            new MaintenanceRequestAssignDto { AssignedToUserId = nonItUser.Id },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Null((await dbContext.MaintenanceRequests.SingleAsync()).AssignedToUserId);
    }

    [Fact]
    public async Task MaintenancePlan_RejectsResponsibleUserWithoutItRole()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var asset = Asset("maintenance-plan-asset", "Stokta");
        var employee = Employee("maintenance-plan-employee");
        var nonItUser = User("maintenance-plan-user", AppRole.Employee);
        nonItUser.EmployeeId = employee.Id;
        dbContext.AddRange(asset, employee, nonItUser);
        await dbContext.SaveChangesAsync();
        var controller = WithUser(
            new MaintenancePlansController(dbContext),
            "admin-user",
            AppRole.Admin);

        var result = await controller.Create(new MaintenancePlanCreateDto
        {
            AssetId = asset.Id,
            Name = "Periyodik kontrol",
            FrequencyDays = 30,
            StartDate = new DateOnly(2026, 9, 1),
            ResponsibleUserId = nonItUser.Id,
            EstimatedDurationMinutes = 30,
            ReminderLeadDays = 5
        }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(dbContext.MaintenancePlans);
    }

    [Fact]
    public async Task EmployeeDashboard_ReturnsOnlyOwnOperationalData()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var employee = Employee("dashboard-owner");
        var otherEmployee = Employee("dashboard-other");
        var asset = Asset("dashboard-owned", "Zimmetli");
        var otherAsset = Asset("dashboard-other-asset", "Zimmetli");
        var actor = User("dashboard-it", AppRole.IT);
        dbContext.AddRange(employee, otherEmployee, asset, otherAsset, actor,
            new Assignment
            {
                Id = "dashboard-own-assignment", AssetId = asset.Id, EmployeeId = employee.Id,
                AssignedAt = DateTimeOffset.UtcNow.AddDays(-3), AssignedByUserId = actor.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3)
            },
            new Assignment
            {
                Id = "dashboard-other-assignment", AssetId = otherAsset.Id, EmployeeId = otherEmployee.Id,
                AssignedAt = DateTimeOffset.UtcNow.AddDays(-2), AssignedByUserId = actor.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new MaintenanceRequest
            {
                Id = "dashboard-own-request", AssetId = asset.Id, RequestedByEmployeeId = employee.Id,
                Title = "Kendi talebi", Description = "Kendi cihazı.",
                Priority = MaintenanceRequestPriority.Normal, Status = MaintenanceRequestStatus.Open,
                CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
            },
            new MaintenanceRequest
            {
                Id = "dashboard-other-request", AssetId = otherAsset.Id, RequestedByEmployeeId = otherEmployee.Id,
                Title = "Diğer talep", Description = "Diğer cihaz.",
                Priority = MaintenanceRequestPriority.High, Status = MaintenanceRequestStatus.InProgress,
                CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
            });
        await dbContext.SaveChangesAsync();

        var summary = await new DashboardService(dbContext).GetMySummaryAsync(
            employee.Id,
            CancellationToken.None);

        Assert.Equal(1, summary.ActiveAssignmentCount);
        Assert.Single(summary.MyAssets);
        Assert.Equal(asset.Id, summary.MyAssets[0].AssetId);
        Assert.Equal(1, summary.OpenSupportRequestCount);
        Assert.Equal(0, summary.InProgressSupportRequestCount);
        Assert.Single(summary.RecentSupportRequests);
        Assert.Equal("dashboard-own-request", summary.RecentSupportRequests[0].Id);
    }

    [Fact]
    public async Task AssetReadModel_UsesActiveAssignmentAsCurrentAssignee()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var employee = Employee("current-assignee");
        var asset = Asset("current-assignee-asset", "Zimmetli");
        var actor = User("current-assignee-it", AppRole.IT);
        var assignedAt = DateTimeOffset.UtcNow.AddDays(-4);
        dbContext.AddRange(employee, asset, actor, new Assignment
        {
            Id = "current-assignment", AssetId = asset.Id, EmployeeId = employee.Id,
            AssignedAt = assignedAt, AssignedByUserId = actor.Id,
            CreatedAt = assignedAt
        });
        await dbContext.SaveChangesAsync();

        var result = await new AssetsController(dbContext).GetById(asset.Id, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<AssetDto>(ok.Value);
        Assert.Equal(employee.Id, dto.CurrentAssigneeEmployeeId);
        Assert.Equal(employee.FullName, dto.CurrentAssigneeName);
        Assert.Equal(employee.Department, dto.CurrentAssigneeDepartment);
        Assert.Equal(assignedAt, dto.CurrentAssignmentDate);
    }

    [Fact]
    public async Task AssignmentService_RejectsSecondActiveAssignment()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var asset = Asset("asset-1", "Stokta");
        var employee = Employee("employee-1");
        dbContext.AddRange(asset, employee, new Assignment
        {
            Id = "assignment-1", AssetId = asset.Id, EmployeeId = employee.Id,
            AssignedAt = DateTimeOffset.UtcNow.AddDays(-1), AssignedByUserId = "user-it",
            CreatedAt = DateTimeOffset.UtcNow, Asset = asset, Employee = employee
        });
        await dbContext.SaveChangesAsync();

        var result = await new AssignmentService(dbContext).CreateAsync(
            new AssignmentCreateDto
            {
                AssetId = asset.Id, EmployeeId = employee.Id,
                AssignedAt = DateTimeOffset.UtcNow
            }, "user-it", CancellationToken.None);

        Assert.Equal(AssignmentOperationStatus.Conflict, result.Status);
        Assert.Contains("aktif", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StockService_DoesNotAllowNegativeStock()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var item = StockItem("stock-1", 1, 0);
        dbContext.StockItems.Add(item);
        await dbContext.SaveChangesAsync();
        var notifications = TestInfrastructure.CreateNotificationService(dbContext);

        var result = await new StockService(dbContext, notifications).CreateTransactionAsync(
            item.Id,
            new StockTransactionCreateDto
            {
                TransactionType = "Çıkış", Quantity = 2,
                TransactionDate = DateTimeOffset.UtcNow
            }, "user-it", CancellationToken.None);

        Assert.Equal(StockTransactionResultStatus.InsufficientStock, result.Status);
        Assert.Equal(1, item.CurrentQuantity);
    }

    [Fact]
    public void LicenseValidation_RejectsUsedSeatsAboveTotalSeats()
    {
        var dto = new LicenseCreateDto
        {
            LicenseCode = "LIC-1", ProductName = "Ürün", Vendor = "Sağlayıcı",
            LicenseType = "Abonelik", TotalSeats = 5, UsedSeats = 6,
            StartDate = new DateOnly(2026, 1, 1), IsActive = true
        };
        var results = Validate(dto);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(dto.UsedSeats)));
    }

    [Fact]
    public void WarrantyRules_ReturnsUpcomingForThirtyDaysOrLess()
    {
        var today = new DateOnly(2026, 8, 15);
        var result = WarrantyRules.Calculate(today.AddDays(30), today);
        Assert.Equal("Yaklaşıyor", result.Status);
        Assert.Equal(30, result.RemainingDays);
    }

    [Fact]
    public void MaintenanceCompletion_RequiresAllCriticalFields()
    {
        var results = Validate(new MaintenanceTaskCompleteDto());
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(MaintenanceTaskCompleteDto.CompletedDate)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(MaintenanceTaskCompleteDto.Result)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(MaintenanceTaskCompleteDto.WorkNotes)));
    }

    [Fact]
    public void MaintenanceRules_CalculatesNextTaskDate()
    {
        Assert.Equal(
            new DateOnly(2026, 11, 13),
            MaintenanceRules.GetNextPlannedDate(new DateOnly(2026, 8, 15), 90));
    }

    [Fact]
    public void JwtTokenService_AddsRoleClaim()
    {
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            Key = "test-only-key-with-at-least-32-characters-123456",
            Issuer = "tests", Audience = "tests", ExpirationMinutes = 10
        }));
        var result = service.CreateToken(new AppUser
        {
            Id = "user-1", Username = "it.test", Email = "it@example.test",
            Role = AppRole.IT, IsActive = true
        });
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.Contains(token.Claims, claim =>
            claim.Type == System.Security.Claims.ClaimTypes.Role && claim.Value == "IT");
    }

    [Theory]
    [InlineData(-1, MaintenanceNotificationType.Overdue)]
    [InlineData(7, MaintenanceNotificationType.Upcoming)]
    public void MaintenanceRules_ClassifiesNotificationType(
        int dayOffset,
        MaintenanceNotificationType expected)
    {
        var today = new DateOnly(2026, 8, 15);
        var task = new MaintenanceTask
        {
            Status = MaintenanceTaskStatus.Planned,
            PlannedDate = today.AddDays(dayOffset)
        };
        Assert.Equal(expected, MaintenanceRules.GetNotificationType(task, today));
    }

    private static IReadOnlyList<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, true);
        return results;
    }

    internal static Asset Asset(string id, string status) => new()
    {
        Id = id, AssetCode = $"AST-{id}", Category = "Test", Brand = "Test",
        Model = "Test", SerialNumber = $"SN-{id}", Status = status,
        Location = "Test", PurchaseDate = new DateOnly(2026, 1, 1),
        WarrantyEndDate = new DateOnly(2027, 1, 1)
    };

    internal static Employee Employee(string id) => new()
    {
        Id = id, EmployeeNo = $"EMP-{id}", FullName = "Test Kullanıcı",
        CorporateEmail = $"{id}@example.test", Department = "Test", IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    internal static StockItem StockItem(string id, int quantity, int minimum) => new()
    {
        Id = id, ItemCode = $"STK-{id}", Name = "Test Ürünü", Category = "Test",
        BrandModel = "Test", Unit = "Adet", CurrentQuantity = quantity,
        MinimumQuantity = minimum, Location = "Test", IsActive = true
    };

    internal static AppUser User(string id, AppRole role) => new()
    {
        Id = id, Username = $"{id}.test", Email = $"{id}@example.test",
        PasswordHash = "test", Role = role, IsActive = true, CreatedAt = DateTimeOffset.UtcNow
    };

    private static TController WithUser<TController>(
        TController controller,
        string userId,
        AppRole role,
        string? employeeId = null)
        where TController : ControllerBase
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, $"{userId}.test"),
            new(ClaimTypes.Role, role.ToString())
        };
        if (employeeId is not null) claims.Add(new Claim("employeeId", employeeId));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
        return controller;
    }
}
