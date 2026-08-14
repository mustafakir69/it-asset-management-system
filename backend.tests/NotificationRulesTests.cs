using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Tests;

public sealed class NotificationRulesTests
{
    [Fact]
    public async Task StockAlert_ResolvesAndAllowsNewAlertAfterRecovery()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var item = CoreBusinessRulesTests.StockItem("recover-1", 2, 3);
        dbContext.StockItems.Add(item);
        await dbContext.SaveChangesAsync();
        var service = TestInfrastructure.CreateNotificationService(dbContext);

        var first = await service.SyncStockAlertAsync(item, CancellationToken.None);
        await dbContext.SaveChangesAsync();
        item.CurrentQuantity = 4;
        await service.SyncStockAlertAsync(item, CancellationToken.None);
        await dbContext.SaveChangesAsync();
        item.CurrentQuantity = 3;
        var second = await service.SyncStockAlertAsync(item, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        Assert.NotNull(first?.ResolvedAt);
        Assert.NotNull(second);
        Assert.Equal(2, dbContext.StockAlerts.Count());
        Assert.Single(dbContext.StockAlerts.Where(alert => alert.ResolvedAt == null));
    }

    [Fact]
    public async Task CriticalStock_DoesNotCreateDuplicateActiveAlert()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var item = CoreBusinessRulesTests.StockItem("critical-1", 2, 3);
        dbContext.StockItems.Add(item);
        await dbContext.SaveChangesAsync();
        var service = TestInfrastructure.CreateNotificationService(dbContext);

        var first = await service.SyncStockAlertAsync(item, CancellationToken.None);
        await dbContext.SaveChangesAsync();
        var second = await service.SyncStockAlertAsync(item, CancellationToken.None);

        Assert.NotNull(first);
        Assert.Null(second);
        Assert.Single(dbContext.StockAlerts);
    }

    [Fact]
    public async Task MaintenanceProcessing_DoesNotCreateDuplicateTypeForTask()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var today = new DateOnly(2026, 8, 15);
        var asset = CoreBusinessRulesTests.Asset("maintenance-asset", "Bakımda");
        var plan = new MaintenancePlan
        {
            Id = "plan-1", AssetId = asset.Id, Asset = asset, Name = "Test Bakımı",
            FrequencyDays = 30, StartDate = today, ResponsibleTechnician = "Test",
            IsActive = true, CreatedAt = DateTimeOffset.UtcNow
        };
        var task = new MaintenanceTask
        {
            Id = "task-1", MaintenancePlanId = plan.Id, MaintenancePlan = plan,
            AssetId = asset.Id, Asset = asset, Title = "Test Bakımı",
            PlannedDate = today.AddDays(2), Status = MaintenanceTaskStatus.Planned,
            CreatedAt = DateTimeOffset.UtcNow
        };
        dbContext.AddRange(asset, plan, task);
        await dbContext.SaveChangesAsync();
        var email = new FakeEmailService();
        var service = TestInfrastructure.CreateNotificationService(dbContext, email);

        var first = await service.ProcessAsync(today, CancellationToken.None);
        var second = await service.ProcessAsync(today, CancellationToken.None);

        Assert.Equal(1, first.MaintenanceNotificationsCreated);
        Assert.Equal(0, second.MaintenanceNotificationsCreated);
        Assert.Single(dbContext.MaintenanceNotifications);
        Assert.Equal(1, email.CallCount);
    }
}
