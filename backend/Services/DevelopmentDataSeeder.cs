using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Services;

public sealed class DevelopmentDataSeeder(
    ApplicationDbContext db,
    IPasswordHasher<AppUser> passwordHasher,
    IConfiguration configuration)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var password = configuration["DevelopmentSeed:TemporaryPassword"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("DevelopmentSeed:TemporaryPassword yalnız Development için user-secrets ile tanımlanmalıdır.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var now = new DateTimeOffset(2026, 8, 15, 9, 0, 0, TimeSpan.Zero);
        var departments = new[] { "Operasyon", "Finans", "İnsan Kaynakları", "Satış", "Muhasebe", "Pazarlama", "Lojistik", "Yönetim", "Satın Alma" };
        var employeeProfiles = new (string FullName, string EmailName)[]
        {
            ("Deniz Aydın", "deniz.aydin"), ("Ece Kaya", "ece.kaya"),
            ("Mert Demir", "mert.demir"), ("Selin Yıldız", "selin.yildiz"),
            ("Can Arslan", "can.arslan"), ("Elif Şahin", "elif.sahin"),
            ("Bora Koç", "bora.koc"), ("İrem Aksoy", "irem.aksoy"),
            ("Ozan Çelik", "ozan.celik"), ("Kerem Tunç", "kerem.tunc"),
            ("Derya Erdem", "derya.erdem"), ("Burak Öz", "burak.oz"),
            ("Ceren Yalçın", "ceren.yalcin"), ("Emre Güneş", "emre.gunes"),
            ("Gizem Acar", "gizem.acar"), ("Hakan Işık", "hakan.isik"),
            ("Jale Kılıç", "jale.kilic"), ("Kaan Polat", "kaan.polat"),
            ("Lale Eren", "lale.eren"), ("Onur Kaplan", "onur.kaplan"),
            ("Pelin Keskin", "pelin.keskin"), ("Rıza Doğan", "riza.dogan"),
            ("Seda Kara", "seda.kara"), ("Tolga Özer", "tolga.ozer"),
            ("Umut Çetin", "umut.cetin"), ("Vildan Kurt", "vildan.kurt"),
            ("Yasemin Tekin", "yasemin.tekin"), ("Zafer Bayrak", "zafer.bayrak"),
            ("Aslı Dinç", "asli.dinc"), ("Barış Sezer", "baris.sezer")
        };

        var employees = await db.Employees.ToDictionaryAsync(x => x.Id, ct);
        for (var number = 1; number <= 30; number++)
        {
            var id = $"employee-db-{number:000}";
            var department = number is 10 or 11 ? "Bilgi İşlem" : departments[(number - 1) % departments.Length];
            var profile = employeeProfiles[number - 1];
            var corporateEmail = $"{profile.EmailName}@example.test";
            if (!employees.TryGetValue(id, out var employee))
            {
                employee = new Employee { Id = id, EmployeeNo = $"EMP-{number:000}", FullName = profile.FullName,
                    CorporateEmail = corporateEmail, Department = department, IsActive = true, CreatedAt = now };
                db.Employees.Add(employee); employees[id] = employee;
            }
            else
            {
                employee.FullName = profile.FullName;
                employee.Department = department;
                employee.IsActive = true;
                employee.CorporateEmail = corporateEmail;
            }
        }
        await db.SaveChangesAsync(ct);

        var allUsers = await db.AppUsers.ToListAsync(ct);
        var admin = allUsers.FirstOrDefault(x => x.Id == "app-user-admin");
        var isNewAdmin = admin is null;
        if (admin is null)
        {
            admin = new AppUser { Id = "app-user-admin", CreatedAt = now };
            db.AppUsers.Add(admin);
            allUsers.Add(admin);
        }

        admin.EmployeeId = null;
        admin.Username = "admin.demo";
        admin.Email = "admin.demo@example.test";
        admin.Role = AppRole.Admin;
        admin.IsActive = true;
        if (isNewAdmin) admin.PasswordHash = passwordHasher.HashPassword(admin, password);

        var seededEmployeeIds = Enumerable.Range(1, 30)
            .Select(number => $"employee-db-{number:000}")
            .ToHashSet(StringComparer.Ordinal);
        var usedUsernames = allUsers
            .Where(user => user.EmployeeId is null ||
                !seededEmployeeIds.Contains(user.EmployeeId) ||
                user.EmployeeId == "employee-db-010")
            .Select(user => user.Username)
            .Where(username => !string.IsNullOrWhiteSpace(username))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var number = 1; number <= 30; number++)
        {
            var employeeId = $"employee-db-{number:000}";
            var role = number is 10 or 11 ? AppRole.IT : AppRole.Employee;
            var userId = number switch { 1 => "app-user-employee", 10 => "app-user-it", _ => $"app-user-employee-{number:000}" };
            var baseUsername = UsernameRules.FromFullName(employees[employeeId].FullName)
                ?? throw new InvalidOperationException($"{employeeId} için kullanıcı adı oluşturulamadı.");
            var username = number == 10
                ? "it.demo"
                : UsernameRules.FirstAvailable(baseUsername, usedUsernames);
            usedUsernames.Add(username);
            var user = allUsers.FirstOrDefault(x => x.EmployeeId == employeeId)
                ?? allUsers.FirstOrDefault(x => x.Id == userId);
            var isNewUser = user is null;

            if (user is null)
            {
                user = new AppUser { Id = userId, CreatedAt = now };
                db.AppUsers.Add(user);
                allUsers.Add(user);
            }

            user.EmployeeId = employeeId;
            user.Username = username;
            user.Email = employees[employeeId].CorporateEmail;
            user.Role = role;
            user.IsActive = true;
            if (isNewUser) user.PasswordHash = passwordHasher.HashPassword(user, password);
        }
        await db.SaveChangesAsync(ct);

        var assets = await db.Assets.ToDictionaryAsync(x => x.Id, ct);
        var assetProfiles = new (string Category, string Brand, string Model)[]
        {
            ("Dizüstü Bilgisayar", "Lenovo", "ThinkPad T14 Gen 5"),
            ("Masaüstü Bilgisayar", "Dell", "OptiPlex 7020"),
            ("Monitör", "HP", "E24 G5"),
            ("Telefon", "Samsung", "Galaxy A55"),
            ("Tablet", "Apple", "iPad 10. Nesil")
        };
        for (var number = 1; number <= 40 && assets.Count < 40; number++)
        {
            var id = $"asset-dev-{number:000}";
            if (assets.ContainsKey(id)) continue;
            var profile = assetProfiles[(number - 1) % assetProfiles.Length];
            var asset = new Asset { Id = id, AssetCode = $"DEV-2026-{number:0000}", Category = profile.Category,
                Brand = profile.Brand, Model = profile.Model, SerialNumber = $"DEV-SN-{number:0000}",
                Status = AssetLifecycleRules.Available, Location = number % 3 == 0 ? "Ankara Ofis" : "İstanbul Merkez",
                PurchaseDate = new DateOnly(2025 + number % 2, 1 + number % 10, 1 + number % 20), WarrantyEndDate = new DateOnly(2028 + number % 2, 1 + number % 10, 1 + number % 20) };
            db.Assets.Add(asset); assets[id] = asset;
        }
        await db.SaveChangesAsync(ct);

        foreach (var asset in assets.Values.Where(item => item.Id.StartsWith("asset-dev-", StringComparison.Ordinal)))
        {
            if (await db.AssetMovements.AnyAsync(
                    movement => movement.AssetId == asset.Id &&
                        movement.MovementType == AssetMovementType.InventoryCreated,
                    ct))
                continue;

            var movement = AssetMovementFactory.Create(
                asset.Id,
                AssetMovementType.InventoryCreated,
                new DateTimeOffset(asset.PurchaseDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
                null,
                AssetLifecycleRules.Available,
                "app-user-it",
                "Development cihazı envantere eklendi.");
            movement.Id = $"asset-movement-dev-created-{asset.Id[^3..]}";
            db.AssetMovements.Add(movement);
        }
        await db.SaveChangesAsync(ct);

        var activeEmployees = await db.Employees.Where(x => x.IsActive).OrderBy(x => x.EmployeeNo).ToListAsync(ct);
        foreach (var employee in activeEmployees)
        {
            if (await db.Assignments.AnyAsync(x => x.EmployeeId == employee.Id && x.ReturnedAt == null, ct)) continue;
            var number = int.Parse(employee.EmployeeNo[4..]);
            var preferredAssetId = $"asset-dev-{number:000}";
            var asset = assets.GetValueOrDefault(preferredAssetId);
            if (asset is null || asset.Status != AssetLifecycleRules.Available ||
                await db.Assignments.AnyAsync(x => x.AssetId == asset.Id && x.ReturnedAt == null, ct))
            {
                asset = assets.Values
                    .Where(x => x.Status == AssetLifecycleRules.Available)
                    .OrderBy(x => x.AssetCode)
                    .FirstOrDefault();
            }
            if (asset is null)
                throw new InvalidOperationException("Development çalışanlarının tümüne zimmetlenecek yeterli cihaz bulunamadı.");

            asset.Status = "Zimmetli";
            db.Assignments.Add(new Assignment { Id = $"assignment-dev-{number:000}", AssetId = asset.Id, EmployeeId = employee.Id,
                AssignedAt = now.AddDays(-number), AssignedByUserId = "app-user-it", Notes = "Development çalışma cihazı zimmeti.", CreatedAt = now });
        }
        await db.SaveChangesAsync(ct);

        var developmentAssignments = await db.Assignments
            .Include(item => item.Employee)
            .Where(item => item.Id.StartsWith("assignment-dev-"))
            .ToListAsync(ct);
        foreach (var assignment in developmentAssignments)
        {
            if (await db.AssetMovements.AnyAsync(
                    movement => movement.RelatedEntityType == nameof(Assignment) &&
                        movement.RelatedEntityId == assignment.Id &&
                        movement.MovementType == AssetMovementType.Assigned,
                    ct))
                continue;

            var movement = AssetMovementFactory.Create(
                assignment.AssetId,
                AssetMovementType.Assigned,
                assignment.AssignedAt,
                AssetLifecycleRules.Available,
                AssetLifecycleRules.Assigned,
                assignment.AssignedByUserId,
                $"Cihaz {assignment.Employee.FullName} adlı çalışana zimmetlendi.",
                relatedEntityType: nameof(Assignment),
                relatedEntityId: assignment.Id);
            movement.Id = $"asset-movement-dev-assigned-{assignment.Id[^3..]}";
            db.AssetMovements.Add(movement);
        }
        await db.SaveChangesAsync(ct);

        var itUser = await db.AppUsers.FirstAsync(x => x.Id == "app-user-it", ct);
        var maintenanceDefinitions = new[]
        {
            (Id: "maintenance-dev-001", AssetNumber: 1, Name: "Dizüstü Bilgisayar 6 Aylık Bakımı", FrequencyDays: 180, Duration: 60),
            (Id: "maintenance-dev-002", AssetNumber: 2, Name: "Güvenlik Güncellemesi Kontrolü", FrequencyDays: 30, Duration: 45),
            (Id: "maintenance-dev-003", AssetNumber: 3, Name: "Donanım ve Bağlantı Kontrolü", FrequencyDays: 90, Duration: 30),
            (Id: "maintenance-dev-004", AssetNumber: 4, Name: "Yedekleme Kontrolü", FrequencyDays: 30, Duration: 30)
        };
        foreach (var definition in maintenanceDefinitions)
        {
            if (await db.MaintenancePlans.AnyAsync(x => x.Id == definition.Id, ct)) continue;
            var asset = assets.GetValueOrDefault($"asset-dev-{definition.AssetNumber:000}")
                ?? assets.Values.OrderBy(x => x.AssetCode).ElementAt(definition.AssetNumber - 1);
            var startDate = DateOnly.FromDateTime(now.UtcDateTime).AddDays(definition.AssetNumber * 3);
            db.MaintenancePlans.Add(new MaintenancePlan
            {
                Id = definition.Id,
                AssetId = asset.Id,
                Name = definition.Name,
                Description = "Development ortamı için ilişkisel periyodik bakım planı.",
                FrequencyDays = definition.FrequencyDays,
                StartDate = startDate,
                ResponsibleUserId = itUser.Id,
                EstimatedDurationMinutes = definition.Duration,
                ReminderLeadDays = 7,
                NextDueAt = startDate,
                IsActive = true,
                CreatedAt = now
            });
            db.MaintenanceTasks.Add(new MaintenanceTask
            {
                Id = $"maintenance-task-dev-{definition.AssetNumber:000}",
                MaintenancePlanId = definition.Id,
                AssetId = asset.Id,
                Title = definition.Name,
                Description = "Development ortamı için planlanan periyodik bakım görevi.",
                PlannedDate = startDate,
                Status = MaintenanceTaskStatus.Planned,
                CreatedAt = now
            });
        }
        await db.SaveChangesAsync(ct);

        var titles = new[] { "Wi-Fi bağlantısı kopuyor", "Monitör görüntü vermiyor", "VPN bağlanmıyor", "Bilgisayar yavaş", "Yazıcıya bağlanamıyor", "Outlook açılmıyor", "Klavye çalışmıyor" };
        for (var number = 1; number <= 12; number++)
        {
            var id = $"support-dev-{number:000}"; if (await db.MaintenanceRequests.AnyAsync(x => x.Id == id, ct)) continue;
            var employee = activeEmployees[(number + 2) % activeEmployees.Count];
            var assignment = await db.Assignments.FirstAsync(x => x.EmployeeId == employee.Id && x.ReturnedAt == null, ct);
            var status = (number % 4) switch { 0 => MaintenanceRequestStatus.Completed, 1 => MaintenanceRequestStatus.Open, 2 => MaintenanceRequestStatus.Assigned, _ => MaintenanceRequestStatus.InProgress };
            db.MaintenanceRequests.Add(new MaintenanceRequest { Id = id, AssetId = assignment.AssetId, RequestedByEmployeeId = employee.Id,
                Title = titles[(number - 1) % titles.Length], Description = "Development ortamı için anonim teknik destek talebi.", Priority = (MaintenanceRequestPriority)(number % 4),
                Status = status, AssignedToUserId = status == MaintenanceRequestStatus.Open ? null : itUser.Id, CreatedAt = now.AddDays(-number), UpdatedAt = now.AddDays(-number + 1),
                CompletedAt = status == MaintenanceRequestStatus.Completed ? now.AddDays(-number + 1) : null,
                CompletedByUserId = status == MaintenanceRequestStatus.Completed ? itUser.Id : null,
                Result = status == MaintenanceRequestStatus.Completed ? "Sorun giderildi ve kullanıcıyla doğrulandı." : null,
                WorkNotes = status == MaintenanceRequestStatus.Completed ? "Kontroller ve gerekli yapılandırma tamamlandı." : null });
        }
        await db.SaveChangesAsync(ct);

        var demoLicense = await db.Licenses
            .Where(item => item.IsActive &&
                (item.ExpirationDate == null || item.ExpirationDate >= DateOnly.FromDateTime(DateTime.Today)))
            .OrderBy(item => item.LicenseCode)
            .FirstOrDefaultAsync(ct);
        if (demoLicense is not null)
        {
            var licenseTargets = new (string Id, string? EmployeeId, string? AssetId)[]
            {
                ("license-assignment-dev-001", activeEmployees[0].Id, null),
                ("license-assignment-dev-002", null, "asset-dev-039"),
                ("license-assignment-dev-003", activeEmployees[1].Id, "asset-dev-040")
            };
            foreach (var target in licenseTargets.Take(demoLicense.TotalSeats))
            {
                if (await db.LicenseAssignments.AnyAsync(item => item.Id == target.Id, ct)) continue;
                db.LicenseAssignments.Add(new LicenseAssignment
                {
                    Id = target.Id,
                    LicenseId = demoLicense.Id,
                    EmployeeId = target.EmployeeId,
                    AssetId = target.AssetId,
                    AssignedAt = now.AddDays(-10 + int.Parse(target.Id[^1..])),
                    AssignedByUserId = itUser.Id
                });
            }
        }

        var deviceLicenseSeeds = new[]
        {
            (Id: "license-assignment-device-dev-040", LicenseCode: "LIC-M365-001", AssetId: "asset-dev-040", DayOffset: -6),
            (Id: "license-assignment-device-dev-039", LicenseCode: "LIC-CAD-007", AssetId: "asset-dev-039", DayOffset: -5),
            (Id: "license-assignment-device-dev-038", LicenseCode: "LIC-ACROBAT-003", AssetId: "asset-dev-038", DayOffset: -4)
        };
        var seededDeviceLicenses = await db.Licenses
            .Where(item => item.IsActive &&
                (item.ExpirationDate == null || item.ExpirationDate >= DateOnly.FromDateTime(DateTime.Today)))
            .ToDictionaryAsync(item => item.LicenseCode, StringComparer.OrdinalIgnoreCase, ct);
        foreach (var seed in deviceLicenseSeeds)
        {
            if (!seededDeviceLicenses.TryGetValue(seed.LicenseCode, out var license) ||
                !assets.ContainsKey(seed.AssetId))
                continue;

            var existing = await db.LicenseAssignments.FirstOrDefaultAsync(
                item => item.Id == seed.Id,
                ct);
            var duplicateActiveAssignment = await db.LicenseAssignments.AnyAsync(
                item => item.Id != seed.Id &&
                    item.LicenseId == license.Id &&
                    item.AssetId == seed.AssetId &&
                    item.EmployeeId == null &&
                    item.RevokedAt == null,
                ct);
            if (duplicateActiveAssignment) continue;

            var activeAssignmentCount = await db.LicenseAssignments.CountAsync(
                item => item.LicenseId == license.Id &&
                    item.RevokedAt == null &&
                    item.Id != seed.Id,
                ct);
            if (activeAssignmentCount >= license.TotalSeats) continue;

            if (existing is null)
            {
                existing = new LicenseAssignment { Id = seed.Id };
                db.LicenseAssignments.Add(existing);
            }

            existing.LicenseId = license.Id;
            existing.EmployeeId = null;
            existing.AssetId = seed.AssetId;
            existing.AssignedAt = now.AddDays(seed.DayOffset);
            existing.AssignedByUserId = itUser.Id;
            existing.RevokedAt = null;
            existing.RevokedByUserId = null;
        }

        var supportRequests = await db.MaintenanceRequests
            .Where(item => item.Id.StartsWith("support-dev-"))
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(ct);
        foreach (var request in supportRequests)
        {
            if (await db.SupportRequestActivities.AnyAsync(
                    item => item.MaintenanceRequestId == request.Id,
                    ct)) continue;
            var employeeUserId = await db.AppUsers
                .Where(item => item.EmployeeId == request.RequestedByEmployeeId)
                .Select(item => item.Id)
                .FirstAsync(ct);
            db.SupportRequestActivities.Add(new SupportRequestActivity
            {
                Id = $"support-activity-{request.Id}-created",
                MaintenanceRequestId = request.Id,
                ActivityType = SupportRequestActivityType.Created,
                OccurredAt = request.CreatedAt,
                PerformedByUserId = employeeUserId,
                NewValue = "Açık",
                Description = "Development teknik destek talebi oluşturuldu."
            });
            if (request.Status == MaintenanceRequestStatus.Open) continue;
            db.SupportRequestActivities.Add(new SupportRequestActivity
            {
                Id = $"support-activity-{request.Id}-assigned",
                MaintenanceRequestId = request.Id,
                ActivityType = SupportRequestActivityType.Assigned,
                OccurredAt = request.CreatedAt.AddHours(1),
                PerformedByUserId = itUser.Id,
                NewValue = itUser.Employee?.FullName ?? itUser.Username,
                Description = "Talep Development IT personeline atandı."
            });
            if (request.Status == MaintenanceRequestStatus.Assigned) continue;
            db.SupportRequestActivities.Add(new SupportRequestActivity
            {
                Id = $"support-activity-{request.Id}-started",
                MaintenanceRequestId = request.Id,
                ActivityType = SupportRequestActivityType.Started,
                OccurredAt = request.CreatedAt.AddHours(2),
                PerformedByUserId = itUser.Id,
                OldValue = "Atandı",
                NewValue = "İşlemde",
                Description = "Talep işleme alındı."
            });
            if (request.Status != MaintenanceRequestStatus.Completed) continue;
            db.SupportRequestActivities.AddRange(
                new SupportRequestActivity
                {
                    Id = $"support-activity-{request.Id}-solution",
                    MaintenanceRequestId = request.Id,
                    ActivityType = SupportRequestActivityType.SolutionAdded,
                    OccurredAt = request.CompletedAt!.Value,
                    PerformedByUserId = itUser.Id,
                    Description = $"Çözüm: {request.Result}"
                },
                new SupportRequestActivity
                {
                    Id = $"support-activity-{request.Id}-completed",
                    MaintenanceRequestId = request.Id,
                    ActivityType = SupportRequestActivityType.Completed,
                    OccurredAt = request.CompletedAt.Value,
                    PerformedByUserId = itUser.Id,
                    OldValue = "İşlemde",
                    NewValue = "Tamamlandı",
                    Description = $"Talep tamamlandı. Çalışma notu: {request.WorkNotes}"
                });
        }

        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }
}
