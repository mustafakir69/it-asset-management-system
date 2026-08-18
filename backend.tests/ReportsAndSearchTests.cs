using System.Text;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Tests;

public sealed class ReportsAndSearchTests
{
    [Fact]
    public async Task NewReports_UseRelationalDataAndDerivedLicenseCounts()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var employee = Employee("report-employee", "Mert Demir", "Finans");
        var itEmployee = Employee("report-it-employee", "Kerem Tunç", "Bilgi İşlem");
        var employeeUser = User("report-employee-user", AppRole.Employee, employee.Id, "mert.demir");
        var itUser = User("report-it-user", AppRole.IT, itEmployee.Id, "kerem.tunc");
        var asset = Asset("report-asset", "DEV-2026-0011", AssetLifecycleRules.Assigned);
        var license = License("report-license", "LIC-ACROBAT-003");
        var request = Request("abcdef1234567890", asset.Id, employee.Id, itUser.Id);
        db.AddRange(
            employee, itEmployee, employeeUser, itUser, asset, license, request,
            new Assignment
            {
                Id = "report-assignment", AssetId = asset.Id, EmployeeId = employee.Id,
                AssignedAt = DateTimeOffset.UtcNow.AddMonths(-1), AssignedByUserId = itUser.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-1)
            },
            new LicenseAssignment
            {
                Id = "report-license-assignment", LicenseId = license.Id, EmployeeId = employee.Id,
                AssetId = asset.Id, AssignedAt = DateTimeOffset.UtcNow, AssignedByUserId = itUser.Id
            });
        await db.SaveChangesAsync();

        var service = new ReportsService(db);
        var warranty = Assert.Single(await service.GetWarrantiesAsync(null, null, CancellationToken.None));
        var licenseReport = Assert.Single(await service.GetLicensesAsync(null, CancellationToken.None));
        var support = Assert.Single(await service.GetSupportRequestsAsync(null, null, CancellationToken.None));

        Assert.Equal("Mert Demir", warranty.CurrentAssigneeName);
        Assert.Equal("Finans", warranty.CurrentAssigneeDepartment);
        Assert.Equal(1, licenseReport.UsedSeats);
        Assert.Equal(4, licenseReport.AvailableSeats);
        Assert.Equal("BT-ABCDEF12", support.RequestNumber);
        Assert.Equal("Mert Demir", support.RequestedByName);
        Assert.Equal("Kerem Tunç", support.AssignedToName);
        var csv = Encoding.UTF8.GetString(CsvExporter.Licenses([licenseReport]));
        Assert.Contains("LIC-ACROBAT-003", csv);
        Assert.Contains("Kullanılan", csv);
    }

    [Fact]
    public async Task GlobalSearch_ReturnsOperationalCategoriesForIt()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var data = await SeedSearchDataAsync(db);
        var service = new GlobalSearchService(db);

        var userResults = await service.SearchAsync("mert.demir", 5, false, null, CancellationToken.None);
        var assetResults = await service.SearchAsync("DEV-2026-0011", 5, false, null, CancellationToken.None);
        var licenseResults = await service.SearchAsync("LIC-ACROBAT-003", 5, false, null, CancellationToken.None);
        var maintenanceResults = await service.SearchAsync("Donanım Kontrolü", 5, false, null, CancellationToken.None);
        var supportResults = await service.SearchAsync("BT-ABCDEF12", 5, false, null, CancellationToken.None);

        Assert.Contains(userResults, item => item.Category == "Kullanıcılar" && item.Title == "Mert Demir");
        Assert.Contains(assetResults, item => item.Category == "Envanter" && item.Title == data.asset.AssetCode);
        Assert.Contains(licenseResults, item => item.Category == "Lisanslar" && item.Title == data.license.LicenseCode);
        Assert.Contains(maintenanceResults, item => item.Category == "Periyodik Bakım");
        Assert.Contains(supportResults, item => item.Category == "Teknik Destek" && item.Title == "BT-ABCDEF12");
        Assert.DoesNotContain(userResults, item => item.Title.Length == 32);
    }

    [Fact]
    public async Task GlobalSearch_EmployeeScopeDoesNotLeakCompanyData()
    {
        await using var db = TestInfrastructure.CreateDbContext();
        var data = await SeedSearchDataAsync(db);
        var service = new GlobalSearchService(db);

        var ownAsset = await service.SearchAsync(data.asset.AssetCode, 5, true, data.employee.Id, CancellationToken.None);
        var license = await service.SearchAsync(data.license.LicenseCode, 5, true, data.employee.Id, CancellationToken.None);
        var anotherAsset = await service.SearchAsync(data.otherAsset.AssetCode, 5, true, data.employee.Id, CancellationToken.None);
        var ownSupport = await service.SearchAsync("BT-ABCDEF12", 5, true, data.employee.Id, CancellationToken.None);

        Assert.Contains(ownAsset, item => item.Category == "Zimmetlerim");
        Assert.Empty(license);
        Assert.Empty(anotherAsset);
        Assert.Contains(ownSupport, item => item.Category == "Teknik Destek");
        Assert.All(ownAsset.Concat(ownSupport), item =>
            Assert.DoesNotContain(item.Category, new[] { "Kullanıcılar", "Lisanslar", "Periyodik Bakım" }));
    }

    private static async Task<(Employee employee, Asset asset, Asset otherAsset, License license)> SeedSearchDataAsync(
        TakipProgrami.Api.Data.ApplicationDbContext db)
    {
        var employee = Employee("search-employee", "Mert Demir", "Finans");
        var otherEmployee = Employee("search-other-employee", "Ece Kaya", "Operasyon");
        var itEmployee = Employee("search-it-employee", "Kerem Tunç", "Bilgi İşlem");
        var employeeUser = User("search-employee-user", AppRole.Employee, employee.Id, "mert.demir");
        var otherUser = User("search-other-user", AppRole.Employee, otherEmployee.Id, "ece.kaya");
        var itUser = User("search-it-user", AppRole.IT, itEmployee.Id, "it.demo");
        var asset = Asset("search-asset", "DEV-2026-0011", AssetLifecycleRules.Assigned);
        var otherAsset = Asset("search-other-asset", "DEV-2026-0099", AssetLifecycleRules.Assigned);
        var license = License("search-license", "LIC-ACROBAT-003");
        var plan = new MaintenancePlan
        {
            Id = "search-plan", AssetId = asset.Id, Name = "Donanım Kontrolü",
            FrequencyDays = 180, StartDate = new DateOnly(2026, 1, 1),
            ResponsibleUserId = itUser.Id, EstimatedDurationMinutes = 30,
            ReminderLeadDays = 5, NextDueAt = new DateOnly(2026, 8, 20),
            IsActive = true, CreatedAt = DateTimeOffset.UtcNow
        };
        db.AddRange(
            employee, otherEmployee, itEmployee, employeeUser, otherUser, itUser,
            asset, otherAsset, license, plan,
            new Assignment { Id = "search-assignment", AssetId = asset.Id, EmployeeId = employee.Id, AssignedAt = DateTimeOffset.UtcNow, AssignedByUserId = itUser.Id, CreatedAt = DateTimeOffset.UtcNow },
            new Assignment { Id = "search-other-assignment", AssetId = otherAsset.Id, EmployeeId = otherEmployee.Id, AssignedAt = DateTimeOffset.UtcNow, AssignedByUserId = itUser.Id, CreatedAt = DateTimeOffset.UtcNow },
            new MaintenanceTask { Id = "search-task", MaintenancePlanId = plan.Id, AssetId = asset.Id, Title = "Donanım Kontrolü", PlannedDate = new DateOnly(2026, 8, 20), Status = MaintenanceTaskStatus.Planned, CreatedAt = DateTimeOffset.UtcNow },
            Request("abcdef1234567890", asset.Id, employee.Id, itUser.Id),
            Request("fedcba9876543210", otherAsset.Id, otherEmployee.Id, itUser.Id));
        await db.SaveChangesAsync();
        return (employee, asset, otherAsset, license);
    }

    private static Employee Employee(string id, string name, string department) => new()
    {
        Id = id, EmployeeNo = "EMP-" + id, FullName = name,
        CorporateEmail = id + "@example.test", Department = department,
        IsActive = true, CreatedAt = DateTimeOffset.UtcNow
    };

    private static AppUser User(string id, AppRole role, string employeeId, string username) => new()
    {
        Id = id, EmployeeId = employeeId, Username = username, Email = username + "@example.test",
        PasswordHash = "test", Role = role, IsActive = true, CreatedAt = DateTimeOffset.UtcNow
    };

    private static Asset Asset(string id, string code, string status) => new()
    {
        Id = id, AssetCode = code, Category = "Dizüstü Bilgisayar", Brand = "Lenovo",
        Model = "ThinkPad", SerialNumber = "SN-" + id, Status = status,
        Location = "İstanbul Merkez", PurchaseDate = new DateOnly(2026, 1, 1),
        WarrantyEndDate = new DateOnly(2027, 1, 1)
    };

    private static License License(string id, string code) => new()
    {
        Id = id, LicenseCode = code, ProductName = "Adobe Acrobat", Vendor = "Adobe",
        LicenseType = "Abonelik", TotalSeats = 5, StartDate = new DateOnly(2026, 1, 1),
        ExpirationDate = new DateOnly(2027, 1, 1), IsActive = true
    };

    private static MaintenanceRequest Request(string id, string assetId, string employeeId, string itUserId) => new()
    {
        Id = id, AssetId = assetId, RequestedByEmployeeId = employeeId,
        Title = "VPN bağlantısı", Description = "VPN bağlantısı kurulamıyor.",
        Priority = MaintenanceRequestPriority.High, Status = MaintenanceRequestStatus.Assigned,
        AssignedToUserId = itUserId, CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
}
