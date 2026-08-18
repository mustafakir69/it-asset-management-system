using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Controllers;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Tests;

public sealed class LicenseAndHistoryTests
{
    [Fact]
    public async Task LicenseAssignments_SupportEmployeeAssetAndCombinedTargets()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var license = License("license-targets", 3);
        var employee = CoreBusinessRulesTests.Employee("license-employee");
        var asset = CoreBusinessRulesTests.Asset("license-asset", "Boşta");
        var actor = CoreBusinessRulesTests.User("license-it", AppRole.IT);
        db.AddRange(license, employee, asset, actor);
        await db.SaveChangesAsync();
        var service = new LicenseAssignmentService(db);

        var employeeOnly = await service.CreateAsync(license.Id, Input(employee.Id, null), actor.Id, CancellationToken.None);
        var assetOnly = await service.CreateAsync(license.Id, Input(null, asset.Id), actor.Id, CancellationToken.None);
        var combined = await service.CreateAsync(license.Id, Input(employee.Id, asset.Id), actor.Id, CancellationToken.None);

        Assert.All([employeeOnly, assetOnly, combined], result => Assert.Equal(LicenseAssignmentOperationStatus.Success, result.Status));
        Assert.Equal(3, await db.LicenseAssignments.CountAsync(item => item.RevokedAt == null));
        Assert.All(await db.LicenseAssignments.ToListAsync(), item => Assert.Equal(actor.Id, item.AssignedByUserId));
        Assert.Equal(asset.Id, assetOnly.Assignment!.AssetId);
        Assert.Null(assetOnly.Assignment.EmployeeId);
        Assert.Equal(asset.Id, combined.Assignment!.AssetId);
        Assert.Equal(employee.Id, combined.Assignment.EmployeeId);
    }

    [Fact]
    public async Task AssetAssignments_ReturnOnlyActiveLicensesForRequestedAsset()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var license = License("asset-query-license", 3);
        var actor = CoreBusinessRulesTests.User("asset-query-it", AppRole.IT);
        var requestedAsset = CoreBusinessRulesTests.Asset("requested-asset", "Boşta");
        var otherAsset = CoreBusinessRulesTests.Asset("other-asset", "Boşta");
        db.AddRange(license, actor, requestedAsset, otherAsset,
            new LicenseAssignment
            {
                Id = "requested-active", LicenseId = license.Id, AssetId = requestedAsset.Id,
                AssignedAt = DateTimeOffset.UtcNow, AssignedByUserId = actor.Id
            },
            new LicenseAssignment
            {
                Id = "requested-revoked", LicenseId = license.Id, AssetId = requestedAsset.Id,
                AssignedAt = DateTimeOffset.UtcNow.AddDays(-2), AssignedByUserId = actor.Id,
                RevokedAt = DateTimeOffset.UtcNow.AddDays(-1), RevokedByUserId = actor.Id
            },
            new LicenseAssignment
            {
                Id = "other-active", LicenseId = license.Id, AssetId = otherAsset.Id,
                AssignedAt = DateTimeOffset.UtcNow, AssignedByUserId = actor.Id
            });
        await db.SaveChangesAsync();

        var result = await new LicenseAssignmentService(db)
            .GetActiveByAssetAsync(requestedAsset.Id, CancellationToken.None);

        var assignment = Assert.Single(result);
        Assert.Equal("requested-active", assignment.Id);
        Assert.Equal(requestedAsset.Id, assignment.AssetId);
        Assert.Equal(requestedAsset.AssetCode, assignment.AssetCode);
    }

    [Fact]
    public async Task LicenseAssignments_RejectEmptyDuplicateCapacityInactiveAndExpired()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var employee = CoreBusinessRulesTests.Employee("license-rules-employee");
        var otherEmployee = CoreBusinessRulesTests.Employee("license-rules-other-employee");
        var actor = CoreBusinessRulesTests.User("license-rules-it", AppRole.IT);
        var active = License("license-capacity", 1);
        var inactive = License("license-inactive", 2); inactive.IsActive = false;
        var expired = License("license-expired", 2); expired.ExpirationDate = new DateOnly(2025, 1, 1);
        db.AddRange(employee, otherEmployee, actor, active, inactive, expired);
        await db.SaveChangesAsync();
        var service = new LicenseAssignmentService(db);

        Assert.Equal(LicenseAssignmentOperationStatus.ValidationError, (await service.CreateAsync(active.Id, Input(null, null), actor.Id, CancellationToken.None)).Status);
        Assert.Equal(LicenseAssignmentOperationStatus.Success, (await service.CreateAsync(active.Id, Input(employee.Id, null), actor.Id, CancellationToken.None)).Status);
        Assert.Equal(LicenseAssignmentOperationStatus.Conflict, (await service.CreateAsync(active.Id, Input(employee.Id, null), actor.Id, CancellationToken.None)).Status);
        Assert.Equal(LicenseAssignmentOperationStatus.Conflict, (await service.CreateAsync(active.Id, Input(otherEmployee.Id, null), actor.Id, CancellationToken.None)).Status);
        Assert.Equal(LicenseAssignmentOperationStatus.Conflict, (await service.CreateAsync(inactive.Id, Input(employee.Id, null), actor.Id, CancellationToken.None)).Status);
        Assert.Equal(LicenseAssignmentOperationStatus.Conflict, (await service.CreateAsync(expired.Id, Input(employee.Id, null), actor.Id, CancellationToken.None)).Status);
    }

    [Fact]
    public async Task LicenseRevocation_PreservesHistoryAndUpdatesCalculatedSeats()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var license = License("license-revoke", 2);
        var employee = CoreBusinessRulesTests.Employee("license-revoke-employee");
        var actor = CoreBusinessRulesTests.User("license-revoke-it", AppRole.IT);
        db.AddRange(license, employee, actor);
        await db.SaveChangesAsync();
        var service = new LicenseAssignmentService(db);
        var created = await service.CreateAsync(license.Id, Input(employee.Id, null), actor.Id, CancellationToken.None);

        var revoked = await service.RevokeAsync(license.Id, created.Assignment!.Id, actor.Id, CancellationToken.None);

        Assert.Equal(LicenseAssignmentOperationStatus.Success, revoked.Status);
        var stored = await db.LicenseAssignments.SingleAsync();
        Assert.NotNull(stored.RevokedAt);
        Assert.Equal(actor.Id, stored.RevokedByUserId);
        Assert.Empty(await service.GetActiveByAssetAsync("missing", CancellationToken.None));
        var controller = new LicensesController(db, service);
        var detail = await controller.GetById(license.Id, CancellationToken.None);
        var dto = Assert.IsType<LicenseDto>(Assert.IsType<OkObjectResult>(detail.Result).Value);
        Assert.Equal(0, dto.UsedSeats);
        Assert.Equal(2, dto.AvailableSeats);
    }

    [Fact]
    public async Task LicenseUpdate_RejectsTotalBelowActiveAssignments()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var license = License("license-total", 2);
        var employee = CoreBusinessRulesTests.Employee("license-total-employee");
        var actor = CoreBusinessRulesTests.User("license-total-it", AppRole.IT);
        db.AddRange(license, employee, actor, new LicenseAssignment
        {
            Id = "license-total-assignment", LicenseId = license.Id, EmployeeId = employee.Id,
            AssignedAt = DateTimeOffset.UtcNow, AssignedByUserId = actor.Id
        });
        await db.SaveChangesAsync();
        var controller = new LicensesController(db, new LicenseAssignmentService(db));

        var result = await controller.Update(license.Id, new LicenseUpdateDto
        {
            LicenseCode = license.LicenseCode, ProductName = license.ProductName, Vendor = license.Vendor,
            LicenseType = license.LicenseType, TotalSeats = 0, StartDate = license.StartDate,
            ExpirationDate = license.ExpirationDate, IsActive = true
        }, CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Contains("aktif atama", Assert.IsType<ProblemDetails>(conflict.Value).Detail);
    }

    [Fact]
    public async Task MaintenanceHistory_FiltersByAsset()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var actor = CoreBusinessRulesTests.User("history-it", AppRole.IT);
        var firstAsset = CoreBusinessRulesTests.Asset("history-first", "Boşta");
        var secondAsset = CoreBusinessRulesTests.Asset("history-second", "Boşta");
        var plan = Plan("history-plan", firstAsset.Id, actor.Id);
        var otherPlan = Plan("history-other-plan", secondAsset.Id, actor.Id);
        db.AddRange(actor, firstAsset, secondAsset, plan, otherPlan,
            MaintenanceTaskEntity("history-task", plan, firstAsset, MaintenanceTaskStatus.Completed, actor.Id),
            MaintenanceTaskEntity("history-other-task", otherPlan, secondAsset, MaintenanceTaskStatus.Cancelled, null));
        await db.SaveChangesAsync();

        var result = await new MaintenanceTasksController(db).GetAll(firstAsset.Id, CancellationToken.None);
        var values = Assert.IsAssignableFrom<IReadOnlyList<MaintenanceTaskDto>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        var task = Assert.Single(values);
        Assert.Equal(firstAsset.Id, task.AssetId);
        Assert.Equal("Tamamlandı", task.Status);
        Assert.Equal(actor.Id, task.CompletedByUserId);
    }

    [Fact]
    public async Task SupportWorkflow_CreatesPersistentActivitiesWithCurrentActors()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var employee = CoreBusinessRulesTests.Employee("activity-employee");
        var asset = CoreBusinessRulesTests.Asset("activity-asset", "Zimmetli");
        var employeeUser = CoreBusinessRulesTests.User("activity-user", AppRole.Employee); employeeUser.EmployeeId = employee.Id;
        var firstIt = CoreBusinessRulesTests.User("activity-it-1", AppRole.IT);
        var secondIt = CoreBusinessRulesTests.User("activity-it-2", AppRole.IT);
        db.AddRange(employee, asset, employeeUser, firstIt, secondIt, new Assignment
        {
            Id = "activity-assignment", AssetId = asset.Id, EmployeeId = employee.Id,
            AssignedAt = DateTimeOffset.UtcNow.AddDays(-1), AssignedByUserId = firstIt.Id,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();
        var employeeController = WithUser(new MaintenanceRequestsController(db), employeeUser, employee.Id);
        var createdResult = await employeeController.Create(new MaintenanceRequestCreateDto
        {
            AssetId = asset.Id, Title = "VPN", Description = "VPN bağlantısı kurulamıyor.", Priority = "Yüksek"
        }, CancellationToken.None);
        var created = Assert.IsType<MaintenanceRequestDto>(Assert.IsType<CreatedAtActionResult>(createdResult.Result).Value);
        var itController = WithUser(new MaintenanceRequestsController(db), firstIt);
        await itController.Assign(created.Id, new MaintenanceRequestAssignDto { AssignedToUserId = firstIt.Id }, CancellationToken.None);
        await itController.Assign(created.Id, new MaintenanceRequestAssignDto { AssignedToUserId = secondIt.Id }, CancellationToken.None);
        await itController.Start(created.Id, CancellationToken.None);
        await itController.Complete(created.Id, new MaintenanceRequestCompleteDto
        {
            CompletedAt = DateTimeOffset.UtcNow, Result = "Yetki düzeltildi.", WorkNotes = "Bağlantı doğrulandı."
        }, CancellationToken.None);

        var activities = await db.SupportRequestActivities.OrderBy(item => item.OccurredAt).ToListAsync();
        Assert.Equal(6, activities.Count);
        Assert.Equal(SupportRequestActivityType.Created, activities[0].ActivityType);
        Assert.Contains(activities, item => item.ActivityType == SupportRequestActivityType.AssigneeChanged);
        Assert.Contains(activities, item => item.ActivityType == SupportRequestActivityType.Started);
        Assert.Contains(activities, item => item.ActivityType == SupportRequestActivityType.SolutionAdded);
        Assert.Contains(activities, item => item.ActivityType == SupportRequestActivityType.Completed);
        Assert.All(activities, item => Assert.Equal(created.Id, item.MaintenanceRequestId));
        Assert.Equal(employeeUser.Id, activities[0].PerformedByUserId);
        Assert.Equal(firstIt.Id, activities[^1].PerformedByUserId);

        var historyResult = await itController.GetActivities(created.Id, CancellationToken.None);
        var history = Assert.IsAssignableFrom<IReadOnlyList<SupportRequestActivityDto>>(
            Assert.IsType<OkObjectResult>(historyResult.Result).Value);
        Assert.Equal(
            ["Talep Oluşturuldu", "IT Personeline Atandı", "Atanan IT Değiştirildi", "İşleme Alındı", "Çözüm Eklendi", "Tamamlandı"],
            history.Select(item => item.ActivityType));
    }

    [Fact]
    public async Task SupportCancellation_CreatesActivityWithoutDeletingHistory()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var employee = CoreBusinessRulesTests.Employee("cancel-employee");
        var asset = CoreBusinessRulesTests.Asset("cancel-asset", "Zimmetli");
        var actor = CoreBusinessRulesTests.User("cancel-it", AppRole.IT);
        var request = new MaintenanceRequest
        {
            Id = "cancel-request", AssetId = asset.Id, RequestedByEmployeeId = employee.Id,
            Title = "Yazıcı", Description = "Bağlantı sorunu", Priority = MaintenanceRequestPriority.Normal,
            Status = MaintenanceRequestStatus.Open, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        };
        db.AddRange(employee, asset, actor, request);
        await db.SaveChangesAsync();

        await WithUser(new MaintenanceRequestsController(db), actor).Cancel(
            request.Id,
            new MaintenanceRequestCancelDto { CancellationReason = "Talep geri çekildi." },
            CancellationToken.None);

        Assert.Equal(MaintenanceRequestStatus.Cancelled, request.Status);
        var activity = await db.SupportRequestActivities.SingleAsync();
        Assert.Equal(SupportRequestActivityType.Cancelled, activity.ActivityType);
        Assert.Equal(actor.Id, activity.PerformedByUserId);
        Assert.NotNull(await db.MaintenanceRequests.FindAsync(request.Id));
    }

    private static LicenseAssignmentCreateDto Input(string? employeeId, string? assetId) => new()
    { EmployeeId = employeeId, AssetId = assetId, AssignedAt = DateTimeOffset.UtcNow };
    private static License License(string id, int totalSeats) => new()
    {
        Id = id, LicenseCode = $"LIC-{id}", ProductName = "Test Lisansı", Vendor = "Test",
        LicenseType = "Abonelik", TotalSeats = totalSeats, StartDate = new DateOnly(2026, 1, 1),
        ExpirationDate = new DateOnly(2027, 1, 1), IsActive = true
    };
    private static MaintenancePlan Plan(string id, string assetId, string userId) => new()
    {
        Id = id, AssetId = assetId, Name = "Bakım", FrequencyDays = 90,
        StartDate = new DateOnly(2026, 1, 1), ResponsibleUserId = userId,
        EstimatedDurationMinutes = 30, ReminderLeadDays = 5, NextDueAt = new DateOnly(2026, 1, 1),
        IsActive = false, CreatedAt = DateTimeOffset.UtcNow
    };
    private static MaintenanceTask MaintenanceTaskEntity(string id, MaintenancePlan plan, Asset asset, MaintenanceTaskStatus status, string? userId) => new()
    {
        Id = id, MaintenancePlanId = plan.Id, AssetId = asset.Id, Title = "Bakım",
        PlannedDate = new DateOnly(2026, 1, 1), CompletedDate = status == MaintenanceTaskStatus.Completed ? new DateOnly(2026, 1, 2) : null,
        CompletedByUserId = userId, Status = status, Result = status == MaintenanceTaskStatus.Completed ? "Tamamlandı" : null,
        WorkNotes = status == MaintenanceTaskStatus.Completed ? "Kontrol edildi" : null,
        CancellationReason = status == MaintenanceTaskStatus.Cancelled ? "Yeniden planlandı" : null,
        CreatedAt = DateTimeOffset.UtcNow, MaintenancePlan = plan, Asset = asset
    };
    private static T WithUser<T>(T controller, AppUser user, string? employeeId = null) where T : ControllerBase
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id), new(ClaimTypes.Name, user.Username), new(ClaimTypes.Role, user.Role.ToString()) };
        if (employeeId is not null) claims.Add(new Claim("employeeId", employeeId));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) } };
        return controller;
    }
}
