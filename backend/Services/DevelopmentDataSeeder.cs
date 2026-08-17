using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.Entities;

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
        admin.PasswordHash = passwordHasher.HashPassword(admin, password);

        for (var number = 1; number <= 30; number++)
        {
            var employeeId = $"employee-db-{number:000}";
            var role = number is 10 or 11 ? AppRole.IT : AppRole.Employee;
            var userId = number switch { 1 => "app-user-employee", 10 => "app-user-it", _ => $"app-user-employee-{number:000}" };
            var username = number switch { 1 => "employee.demo", 10 => "it.demo", 11 => "it2.demo", _ => $"employee{number:00}.demo" };
            var user = allUsers.FirstOrDefault(x => x.EmployeeId == employeeId)
                ?? allUsers.FirstOrDefault(x => x.Id == userId);

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
            user.PasswordHash = passwordHasher.HashPassword(user, password);
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
                Status = "Stokta", Location = number % 3 == 0 ? "Ankara Ofis" : "İstanbul Merkez",
                PurchaseDate = new DateOnly(2025 + number % 2, 1 + number % 10, 1 + number % 20), WarrantyEndDate = new DateOnly(2028 + number % 2, 1 + number % 10, 1 + number % 20) };
            db.Assets.Add(asset); assets[id] = asset;
        }
        await db.SaveChangesAsync(ct);

        var activeEmployees = await db.Employees.Where(x => x.IsActive).OrderBy(x => x.EmployeeNo).ToListAsync(ct);
        foreach (var employee in activeEmployees)
        {
            if (await db.Assignments.AnyAsync(x => x.EmployeeId == employee.Id && x.ReturnedAt == null, ct)) continue;
            var number = int.Parse(employee.EmployeeNo[4..]);
            var preferredAssetId = $"asset-dev-{number:000}";
            var asset = assets.GetValueOrDefault(preferredAssetId);
            if (asset is null || asset.Status != "Stokta" ||
                await db.Assignments.AnyAsync(x => x.AssetId == asset.Id && x.ReturnedAt == null, ct))
            {
                asset = assets.Values
                    .Where(x => x.Status == "Stokta")
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
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }
}
