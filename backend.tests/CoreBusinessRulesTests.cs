using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Tests;

public sealed class CoreBusinessRulesTests
{
    [Fact]
    public async Task AssignmentService_RejectsSecondActiveAssignment()
    {
        await using var dbContext = TestInfrastructure.CreateDbContext();
        var asset = Asset("asset-1", "Stokta");
        var employee = Employee("employee-1");
        dbContext.AddRange(asset, employee, new Assignment
        {
            Id = "assignment-1", AssetId = asset.Id, EmployeeId = employee.Id,
            AssignedAt = DateTimeOffset.UtcNow.AddDays(-1), AssignedBy = "Test",
            CreatedAt = DateTimeOffset.UtcNow, Asset = asset, Employee = employee
        });
        await dbContext.SaveChangesAsync();

        var result = await new AssignmentService(dbContext).CreateAsync(
            new AssignmentCreateDto
            {
                AssetId = asset.Id, EmployeeId = employee.Id,
                AssignedAt = DateTimeOffset.UtcNow, AssignedBy = "Test"
            }, CancellationToken.None);

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
                TransactionDate = DateTimeOffset.UtcNow, PersonName = "Test"
            }, CancellationToken.None);

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
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(MaintenanceTaskCompleteDto.CompletedBy)));
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
            Id = "user-1", Username = "auditor.test", Email = "auditor@example.test",
            Role = AppRole.Auditor, IsActive = true
        });
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.Contains(token.Claims, claim =>
            claim.Type == System.Security.Claims.ClaimTypes.Role && claim.Value == "Auditor");
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
        Email = $"{id}@example.test", Department = "Test", IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    internal static StockItem StockItem(string id, int quantity, int minimum) => new()
    {
        Id = id, ItemCode = $"STK-{id}", Name = "Test Ürünü", Category = "Test",
        BrandModel = "Test", Unit = "Adet", CurrentQuantity = quantity,
        MinimumQuantity = minimum, Location = "Test", IsActive = true
    };
}
