using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Data;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IHttpContextAccessor httpContextAccessor)
    : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<MaintenancePlan> MaintenancePlans => Set<MaintenancePlan>();
    public DbSet<MaintenanceTask> MaintenanceTasks => Set<MaintenanceTask>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<StockAlert> StockAlerts => Set<StockAlert>();
    public DbSet<MaintenanceNotification> MaintenanceNotifications => Set<MaintenanceNotification>();

    private static readonly JsonSerializerOptions AuditJsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly HashSet<string> SensitivePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password",
        "PasswordHash",
        "JwtKey",
        "Token"
    };

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        AddAuditEntries();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var asset = modelBuilder.Entity<Asset>();

        asset.ToTable("Assets");
        asset.HasKey(item => item.Id);

        asset.Property(item => item.Id).HasMaxLength(64);
        asset.Property(item => item.AssetCode).HasMaxLength(50).IsRequired();
        asset.Property(item => item.Category).HasMaxLength(100).IsRequired();
        asset.Property(item => item.Brand).HasMaxLength(100).IsRequired();
        asset.Property(item => item.Model).HasMaxLength(150).IsRequired();
        asset.Property(item => item.SerialNumber).HasMaxLength(100).IsRequired();
        asset.Property(item => item.Status).HasMaxLength(50).IsRequired();
        asset.Property(item => item.Location).HasMaxLength(150).IsRequired();
        asset.Property(item => item.PurchaseDate).HasColumnType("date");
        asset.Property(item => item.WarrantyEndDate).HasColumnType("date");

        asset.HasIndex(item => item.AssetCode)
            .IsUnique()
            .HasDatabaseName("UX_Assets_AssetCode");

        asset.HasIndex(item => item.SerialNumber)
            .IsUnique()
            .HasDatabaseName("UX_Assets_SerialNumber");

        IgnoreLegacyDemoData(
            new Asset
            {
                Id = "asset-db-001",
                AssetCode = "DNT-2026-1001",
                Category = "Dizüstü Bilgisayar",
                Brand = "Lenovo",
                Model = "ThinkPad T14 Gen 5",
                SerialNumber = "DB-LNV-T14-1001",
                Status = "Zimmetli",
                Location = "İstanbul Merkez",
                PurchaseDate = new DateOnly(2026, 2, 12),
                WarrantyEndDate = new DateOnly(2029, 2, 12)
            },
            new Asset
            {
                Id = "asset-db-002",
                AssetCode = "DNT-2026-1002",
                Category = "Dizüstü Bilgisayar",
                Brand = "Dell",
                Model = "Latitude 5450",
                SerialNumber = "DB-DLL-L54-1002",
                Status = "Stokta",
                Location = "Ankara Ofis",
                PurchaseDate = new DateOnly(2026, 1, 28),
                WarrantyEndDate = new DateOnly(2029, 1, 28)
            },
            new Asset
            {
                Id = "asset-db-003",
                AssetCode = "DNT-2025-1003",
                Category = "Dizüstü Bilgisayar",
                Brand = "HP",
                Model = "EliteBook 840 G11",
                SerialNumber = "DB-HP-840-1003",
                Status = "Bakımda",
                Location = "İstanbul Merkez",
                PurchaseDate = new DateOnly(2025, 10, 6),
                WarrantyEndDate = new DateOnly(2028, 10, 6)
            },
            new Asset
            {
                Id = "asset-db-004",
                AssetCode = "MST-2026-1004",
                Category = "Masaüstü Bilgisayar",
                Brand = "Dell",
                Model = "OptiPlex 7020",
                SerialNumber = "DB-DLL-OP7-1004",
                Status = "Stokta",
                Location = "İstanbul Depo",
                PurchaseDate = new DateOnly(2026, 3, 14),
                WarrantyEndDate = new DateOnly(2029, 3, 14)
            },
            new Asset
            {
                Id = "asset-db-005",
                AssetCode = "MST-2024-1005",
                Category = "Masaüstü Bilgisayar",
                Brand = "HP",
                Model = "Pro Tower 400 G9",
                SerialNumber = "DB-HP-400-1005",
                Status = "Hurda",
                Location = "İstanbul Depo",
                PurchaseDate = new DateOnly(2021, 5, 17),
                WarrantyEndDate = new DateOnly(2024, 5, 17)
            },
            new Asset
            {
                Id = "asset-db-006",
                AssetCode = "MNT-2026-1006",
                Category = "Monitör",
                Brand = "Dell",
                Model = "P2725H 27 inç",
                SerialNumber = "DB-DLL-P27-1006",
                Status = "Stokta",
                Location = "İstanbul Depo",
                PurchaseDate = new DateOnly(2026, 4, 8),
                WarrantyEndDate = new DateOnly(2029, 4, 8)
            },
            new Asset
            {
                Id = "asset-db-007",
                AssetCode = "MNT-2025-1007",
                Category = "Monitör",
                Brand = "Samsung",
                Model = "ViewFinity S6 27 inç",
                SerialNumber = "DB-SMS-S6-1007",
                Status = "Kayıp",
                Location = "İzmir Ofis",
                PurchaseDate = new DateOnly(2025, 12, 15),
                WarrantyEndDate = new DateOnly(2028, 12, 15)
            },
            new Asset
            {
                Id = "asset-db-008",
                AssetCode = "TLF-2026-1008",
                Category = "Telefon",
                Brand = "Apple",
                Model = "iPhone 16",
                SerialNumber = "DB-APL-IP16-1008",
                Status = "Zimmetli",
                Location = "İstanbul Merkez",
                PurchaseDate = new DateOnly(2026, 2, 2),
                WarrantyEndDate = new DateOnly(2028, 2, 2)
            },
            new Asset
            {
                Id = "asset-db-009",
                AssetCode = "TLF-2023-1009",
                Category = "Telefon",
                Brand = "Samsung",
                Model = "Galaxy S22",
                SerialNumber = "DB-SMS-S22-1009",
                Status = "Elden çıkarıldı",
                Location = "İstanbul Depo",
                PurchaseDate = new DateOnly(2022, 3, 18),
                WarrantyEndDate = new DateOnly(2024, 3, 18)
            },
            new Asset
            {
                Id = "asset-db-010",
                AssetCode = "YAZ-2026-1010",
                Category = "Yazıcı",
                Brand = "Canon",
                Model = "i-SENSYS MF655Cdw",
                SerialNumber = "DB-CNN-MF6-1010",
                Status = "Stokta",
                Location = "İstanbul Depo",
                PurchaseDate = new DateOnly(2026, 5, 6),
                WarrantyEndDate = new DateOnly(2028, 5, 6)
            });

        ConfigureEmployeesAndAssignments(modelBuilder);
        ConfigureAppUsers(modelBuilder);
        ConfigureStockItems(modelBuilder);
        ConfigureStockTransactions(modelBuilder);
        ConfigureLicenses(modelBuilder);
        ConfigureMaintenance(modelBuilder);
        ConfigureAuditLogs(modelBuilder);
        ConfigureNotifications(modelBuilder);
    }

    private static void ConfigureNotifications(ModelBuilder modelBuilder)
    {
        var stockAlert = modelBuilder.Entity<StockAlert>();
        stockAlert.ToTable("StockAlerts");
        stockAlert.HasKey(item => item.Id);
        stockAlert.Property(item => item.Id).HasMaxLength(64);
        stockAlert.Property(item => item.StockItemId).HasMaxLength(64).IsRequired();
        stockAlert.Property(item => item.TriggeredAt).HasColumnType("datetimeoffset");
        stockAlert.Property(item => item.Recipient).HasMaxLength(254).IsRequired();
        stockAlert.Property(item => item.SentAt).HasColumnType("datetimeoffset");
        stockAlert.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        stockAlert.Property(item => item.ErrorMessage).HasMaxLength(2000);
        stockAlert.Property(item => item.ResolvedAt).HasColumnType("datetimeoffset");
        stockAlert.HasIndex(item => item.StockItemId)
            .IsUnique()
            .HasFilter("[ResolvedAt] IS NULL")
            .HasDatabaseName("UX_StockAlerts_StockItemId_Active");
        stockAlert.HasIndex(item => item.TriggeredAt)
            .HasDatabaseName("IX_StockAlerts_TriggeredAt");
        stockAlert.HasOne(item => item.StockItem)
            .WithMany(item => item.Alerts)
            .HasForeignKey(item => item.StockItemId)
            .OnDelete(DeleteBehavior.Restrict);

        var maintenanceNotification = modelBuilder.Entity<MaintenanceNotification>();
        maintenanceNotification.ToTable("MaintenanceNotifications");
        maintenanceNotification.HasKey(item => item.Id);
        maintenanceNotification.Property(item => item.Id).HasMaxLength(64);
        maintenanceNotification.Property(item => item.MaintenanceTaskId).HasMaxLength(64).IsRequired();
        maintenanceNotification.Property(item => item.NotificationType)
            .HasConversion<string>().HasMaxLength(30).IsRequired();
        maintenanceNotification.Property(item => item.Recipient).HasMaxLength(254).IsRequired();
        maintenanceNotification.Property(item => item.ScheduledAt).HasColumnType("datetimeoffset");
        maintenanceNotification.Property(item => item.SentAt).HasColumnType("datetimeoffset");
        maintenanceNotification.Property(item => item.DeliveryStatus)
            .HasConversion<string>().HasMaxLength(30).IsRequired();
        maintenanceNotification.Property(item => item.ErrorMessage).HasMaxLength(2000);
        maintenanceNotification.HasIndex(item => new { item.MaintenanceTaskId, item.NotificationType })
            .IsUnique()
            .HasDatabaseName("UX_MaintenanceNotifications_Task_Type");
        maintenanceNotification.HasIndex(item => item.ScheduledAt)
            .HasDatabaseName("IX_MaintenanceNotifications_ScheduledAt");
        maintenanceNotification.HasOne(item => item.MaintenanceTask)
            .WithMany(item => item.Notifications)
            .HasForeignKey(item => item.MaintenanceTaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAuditLogs(ModelBuilder modelBuilder)
    {
        var auditLog = modelBuilder.Entity<AuditLog>();
        auditLog.ToTable("AuditLogs");
        auditLog.HasKey(item => item.Id);
        auditLog.Property(item => item.Id).HasMaxLength(64);
        auditLog.Property(item => item.UserId).HasMaxLength(64).IsRequired();
        auditLog.Property(item => item.Username).HasMaxLength(100).IsRequired();
        auditLog.Property(item => item.EntityName).HasMaxLength(100).IsRequired();
        auditLog.Property(item => item.EntityId).HasMaxLength(64).IsRequired();
        auditLog.Property(item => item.Action).HasMaxLength(100).IsRequired();
        auditLog.Property(item => item.OldValue).HasColumnType("nvarchar(max)");
        auditLog.Property(item => item.NewValue).HasColumnType("nvarchar(max)");
        auditLog.Property(item => item.CreatedAt).HasColumnType("datetimeoffset");
        auditLog.HasIndex(item => item.CreatedAt)
            .HasDatabaseName("IX_AuditLogs_CreatedAt");
        auditLog.HasIndex(item => new { item.EntityName, item.Action })
            .HasDatabaseName("IX_AuditLogs_EntityName_Action");
        auditLog.HasIndex(item => item.Username)
            .HasDatabaseName("IX_AuditLogs_Username");
    }

    private void AddAuditEntries()
    {
        ChangeTracker.DetectChanges();
        var entries = ChangeTracker.Entries()
            .Where(entry => IsAuditedEntry(entry))
            .ToList();
        if (entries.Count == 0) return;

        var principal = httpContextAccessor.HttpContext?.User;
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        var username = principal?.FindFirstValue(ClaimTypes.Name) ?? "system";
        var createdAt = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            var changedProperties = entry.State == EntityState.Added
                ? entry.Properties.Where(IsSafeProperty).ToList()
                : entry.Properties.Where(property => property.IsModified && IsSafeProperty(property)).ToList();
            if (changedProperties.Count == 0) continue;

            var oldValues = entry.State == EntityState.Modified
                ? changedProperties.ToDictionary(
                    property => property.Metadata.Name,
                    property => property.OriginalValue)
                : null;
            var newValues = changedProperties.ToDictionary(
                property => property.Metadata.Name,
                property => property.CurrentValue);

            AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = userId,
                Username = username,
                EntityName = entry.Metadata.ClrType.Name,
                EntityId = entry.Property("Id").CurrentValue?.ToString() ?? string.Empty,
                Action = ResolveAction(entry),
                OldValue = oldValues is null ? null : JsonSerializer.Serialize(oldValues, AuditJsonOptions),
                NewValue = JsonSerializer.Serialize(newValues, AuditJsonOptions),
                CreatedAt = createdAt
            });
        }
    }

    private static bool IsAuditedEntry(EntityEntry entry) =>
        entry.State is EntityState.Added or EntityState.Modified &&
        entry.Entity switch
        {
            Asset => true,
            Assignment => true,
            StockTransaction => entry.State == EntityState.Added,
            License => true,
            MaintenanceTask => true,
            MaintenanceRequest => true,
            AppUser => entry.State == EntityState.Added,
            StockAlert => true,
            MaintenanceNotification => true,
            _ => false
        };

    private static bool IsSafeProperty(PropertyEntry property) =>
        !SensitivePropertyNames.Contains(property.Metadata.Name) &&
        !property.Metadata.IsShadowProperty();

    private static string ResolveAction(EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
        {
            return entry.Entity switch
            {
                Assignment => "Zimmet Oluşturma",
                StockTransaction => "Stok Hareketi",
                AppUser => "Kullanıcı Oluşturma",
                MaintenanceRequest => "Talep Oluşturma",
                StockAlert => "Kritik Stok Bildirimi",
                MaintenanceNotification => "Bakım Bildirimi",
                _ => "Oluşturma"
            };
        }

        if (entry.Entity is Assignment && entry.Property(nameof(Assignment.ReturnedAt)).IsModified)
            return "İade";

        if (entry.Entity is MaintenanceTask task)
        {
            if (entry.Property(nameof(MaintenanceTask.Status)).IsModified)
            {
                return task.Status switch
                {
                    MaintenanceTaskStatus.Completed => "Tamamlama",
                    MaintenanceTaskStatus.Cancelled => "İptal",
                    _ => "Durum Güncelleme"
                };
            }
            if (entry.Property(nameof(MaintenanceTask.PlannedDate)).IsModified)
                return "Yeniden Planlama";
        }

        if (entry.Entity is MaintenanceRequest request)
        {
            if (entry.Property(nameof(MaintenanceRequest.Status)).IsModified)
            {
                return request.Status switch
                {
                    MaintenanceRequestStatus.Assigned => "Atama",
                    MaintenanceRequestStatus.InProgress => "İşleme Alma",
                    MaintenanceRequestStatus.Completed => "Tamamlama",
                    MaintenanceRequestStatus.Cancelled => "İptal",
                    _ => "Durum Güncelleme"
                };
            }
            return "Talep Güncelleme";
        }

        return "Güncelleme";
    }

    private static void ConfigureEmployeesAndAssignments(ModelBuilder modelBuilder)
    {
        var employee = modelBuilder.Entity<Employee>();

        employee.ToTable("Employees");
        employee.HasKey(item => item.Id);
        employee.Property(item => item.Id).HasMaxLength(64);
        employee.Property(item => item.EmployeeNo).HasMaxLength(50).IsRequired();
        employee.Property(item => item.FullName).HasMaxLength(150).IsRequired();
        employee.Property(item => item.CorporateEmail).HasMaxLength(254).IsRequired();
        employee.Property(item => item.Department).HasMaxLength(100).IsRequired();
        employee.Property(item => item.CreatedAt).HasColumnType("datetimeoffset");

        employee.HasIndex(item => item.EmployeeNo)
            .IsUnique()
            .HasDatabaseName("UX_Employees_EmployeeNo");

        employee.HasIndex(item => item.CorporateEmail)
            .IsUnique()
            .HasDatabaseName("UX_Employees_Email");

        var employeeCreatedAt = new DateTimeOffset(2026, 8, 1, 8, 0, 0, TimeSpan.Zero);

        IgnoreLegacyDemoData(
            new Employee { Id = "employee-db-001", EmployeeNo = "EMP-001", FullName = "Demo Kullanıcı 1", CorporateEmail = "demo.kullanici1@example.test", Department = "Bilgi İşlem", IsActive = true, CreatedAt = employeeCreatedAt },
            new Employee { Id = "employee-db-002", EmployeeNo = "EMP-002", FullName = "Demo Kullanıcı 2", CorporateEmail = "demo.kullanici2@example.test", Department = "Finans", IsActive = true, CreatedAt = employeeCreatedAt },
            new Employee { Id = "employee-db-003", EmployeeNo = "EMP-003", FullName = "Demo Kullanıcı 3", CorporateEmail = "demo.kullanici3@example.test", Department = "İnsan Kaynakları", IsActive = true, CreatedAt = employeeCreatedAt },
            new Employee { Id = "employee-db-004", EmployeeNo = "EMP-004", FullName = "Demo Kullanıcı 4", CorporateEmail = "demo.kullanici4@example.test", Department = "Operasyon", IsActive = true, CreatedAt = employeeCreatedAt },
            new Employee { Id = "employee-db-005", EmployeeNo = "EMP-005", FullName = "Demo Kullanıcı 5", CorporateEmail = "demo.kullanici5@example.test", Department = "Satış", IsActive = true, CreatedAt = employeeCreatedAt },
            new Employee { Id = "employee-db-006", EmployeeNo = "EMP-006", FullName = "Demo Kullanıcı 6", CorporateEmail = "demo.kullanici6@example.test", Department = "Muhasebe", IsActive = true, CreatedAt = employeeCreatedAt },
            new Employee { Id = "employee-db-007", EmployeeNo = "EMP-007", FullName = "Demo Kullanıcı 7", CorporateEmail = "demo.kullanici7@example.test", Department = "Pazarlama", IsActive = true, CreatedAt = employeeCreatedAt },
            new Employee { Id = "employee-db-008", EmployeeNo = "EMP-008", FullName = "Demo Kullanıcı 8", CorporateEmail = "demo.kullanici8@example.test", Department = "Lojistik", IsActive = true, CreatedAt = employeeCreatedAt },
            new Employee { Id = "employee-db-009", EmployeeNo = "EMP-009", FullName = "Demo Kullanıcı 9", CorporateEmail = "demo.kullanici9@example.test", Department = "Operasyon", IsActive = false, CreatedAt = employeeCreatedAt });

        var assignment = modelBuilder.Entity<Assignment>();

        assignment.ToTable("Assignments", table =>
            table.HasCheckConstraint(
                "CK_Assignments_ReturnedAt_NotBeforeAssignedAt",
                "[ReturnedAt] IS NULL OR [ReturnedAt] >= [AssignedAt]"));
        assignment.HasKey(item => item.Id);
        assignment.Property(item => item.Id).HasMaxLength(64);
        assignment.Property(item => item.AssetId).HasMaxLength(64).IsRequired();
        assignment.Property(item => item.EmployeeId).HasMaxLength(64).IsRequired();
        assignment.Property(item => item.AssignedAt).HasColumnType("datetimeoffset");
        assignment.Property(item => item.ReturnedAt).HasColumnType("datetimeoffset");
        assignment.Property(item => item.AssignedByUserId).HasMaxLength(64).IsRequired();
        assignment.Property(item => item.ReturnedByUserId).HasMaxLength(64);
        assignment.Property(item => item.Notes).HasMaxLength(1000);
        assignment.Property(item => item.ReturnNotes).HasMaxLength(1000);
        assignment.Property(item => item.CreatedAt).HasColumnType("datetimeoffset");

        assignment.HasIndex(item => item.AssetId)
            .IsUnique()
            .HasFilter("[ReturnedAt] IS NULL")
            .HasDatabaseName("UX_Assignments_AssetId_Active");

        assignment.HasIndex(item => item.EmployeeId)
            .HasDatabaseName("IX_Assignments_EmployeeId");

        assignment.HasOne(item => item.Asset)
            .WithMany(asset => asset.Assignments)
            .HasForeignKey(item => item.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        assignment.HasOne(item => item.Employee)
            .WithMany(employeeItem => employeeItem.Assignments)
            .HasForeignKey(item => item.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        assignment.HasOne(item => item.AssignedByUser)
            .WithMany(user => user.AssignmentsCreated)
            .HasForeignKey(item => item.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        assignment.HasOne(item => item.ReturnedByUser)
            .WithMany(user => user.AssignmentsReturned)
            .HasForeignKey(item => item.ReturnedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        var assignmentCreatedAt = new DateTimeOffset(2026, 8, 1, 9, 0, 0, TimeSpan.Zero);

        IgnoreLegacyDemoData(
            new Assignment { Id = "assignment-db-001", AssetId = "asset-db-001", EmployeeId = "employee-db-001", AssignedAt = new DateTimeOffset(2026, 3, 2, 9, 0, 0, TimeSpan.Zero), AssignedByUserId = "app-user-it", Notes = "Standart çalışma cihazı zimmeti.", CreatedAt = assignmentCreatedAt },
            new Assignment { Id = "assignment-db-002", AssetId = "asset-db-008", EmployeeId = "employee-db-002", AssignedAt = new DateTimeOffset(2026, 4, 6, 10, 0, 0, TimeSpan.Zero), AssignedByUserId = "app-user-it", Notes = "Kurumsal telefon zimmeti.", CreatedAt = assignmentCreatedAt },
            new Assignment { Id = "assignment-db-003", AssetId = "asset-db-002", EmployeeId = "employee-db-003", AssignedAt = new DateTimeOffset(2026, 2, 10, 9, 30, 0, TimeSpan.Zero), ReturnedAt = new DateTimeOffset(2026, 5, 15, 16, 0, 0, TimeSpan.Zero), AssignedByUserId = "app-user-it", ReturnedByUserId = "app-user-it", Notes = "Geçici proje cihazı.", ReturnNotes = "Eksiksiz teslim alındı.", CreatedAt = assignmentCreatedAt },
            new Assignment { Id = "assignment-db-004", AssetId = "asset-db-004", EmployeeId = "employee-db-004", AssignedAt = new DateTimeOffset(2026, 3, 20, 8, 45, 0, TimeSpan.Zero), ReturnedAt = new DateTimeOffset(2026, 6, 30, 17, 0, 0, TimeSpan.Zero), AssignedByUserId = "app-user-it", ReturnedByUserId = "app-user-it", Notes = "Operasyon masası kullanımı.", ReturnNotes = "Cihaz depoya alındı.", CreatedAt = assignmentCreatedAt },
            new Assignment { Id = "assignment-db-005", AssetId = "asset-db-006", EmployeeId = "employee-db-005", AssignedAt = new DateTimeOffset(2026, 4, 12, 9, 0, 0, TimeSpan.Zero), ReturnedAt = new DateTimeOffset(2026, 7, 18, 15, 30, 0, TimeSpan.Zero), AssignedByUserId = "app-user-it", ReturnedByUserId = "app-user-it", Notes = "Ek çalışma ekranı.", ReturnNotes = "Fiziksel kontrol tamamlandı.", CreatedAt = assignmentCreatedAt },
            new Assignment { Id = "assignment-db-006", AssetId = "asset-db-010", EmployeeId = "employee-db-006", AssignedAt = new DateTimeOffset(2026, 5, 8, 11, 0, 0, TimeSpan.Zero), ReturnedAt = new DateTimeOffset(2026, 7, 22, 14, 0, 0, TimeSpan.Zero), AssignedByUserId = "app-user-it", ReturnedByUserId = "app-user-it", Notes = "Birim yazıcısı kullanımı.", ReturnNotes = "Çalışır durumda teslim alındı.", CreatedAt = assignmentCreatedAt },
            new Assignment { Id = "assignment-db-007", AssetId = "asset-db-003", EmployeeId = "employee-db-007", AssignedAt = new DateTimeOffset(2025, 11, 3, 9, 0, 0, TimeSpan.Zero), ReturnedAt = new DateTimeOffset(2026, 1, 9, 16, 30, 0, TimeSpan.Zero), AssignedByUserId = "app-user-it", ReturnedByUserId = "app-user-it", Notes = "Dönemsel kullanım.", ReturnNotes = "Bakım kontrolü için teslim alındı.", CreatedAt = assignmentCreatedAt },
            new Assignment { Id = "assignment-db-008", AssetId = "asset-db-007", EmployeeId = "employee-db-008", AssignedAt = new DateTimeOffset(2026, 1, 12, 10, 0, 0, TimeSpan.Zero), ReturnedAt = new DateTimeOffset(2026, 2, 20, 15, 0, 0, TimeSpan.Zero), AssignedByUserId = "app-user-it", ReturnedByUserId = "app-user-it", Notes = "Geçici monitör kullanımı.", ReturnNotes = "Teslim kaydı tamamlandı.", CreatedAt = assignmentCreatedAt });
    }

    private static void ConfigureAppUsers(ModelBuilder modelBuilder)
    {
        var appUser = modelBuilder.Entity<AppUser>();

        appUser.ToTable("AppUsers", table =>
        {
            table.HasCheckConstraint(
                "CK_AppUsers_Role_Valid",
                "[Role] IN ('Admin', 'IT', 'Employee')");
            table.HasCheckConstraint(
                "CK_AppUsers_ActiveEmployee_Link",
                "[IsActive] = 0 OR [Role] = 'Admin' OR [EmployeeId] IS NOT NULL");
        });
        appUser.HasKey(user => user.Id);
        appUser.Property(user => user.Id).HasMaxLength(64);
        appUser.Property(user => user.EmployeeId).HasMaxLength(64);
        appUser.Property(user => user.Username).HasMaxLength(100).IsRequired();
        appUser.Property(user => user.Email).HasMaxLength(254).IsRequired();
        appUser.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
        appUser.Property(user => user.Role).HasConversion<string>().HasMaxLength(30).IsRequired();
        appUser.Property(user => user.CreatedAt).HasColumnType("datetimeoffset");
        appUser.Property(user => user.LastLoginAt).HasColumnType("datetimeoffset");

        appUser.HasIndex(user => user.Username)
            .IsUnique()
            .HasDatabaseName("UX_AppUsers_Username");
        appUser.HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("UX_AppUsers_Email");
        appUser.HasIndex(user => user.EmployeeId)
            .IsUnique()
            .HasFilter("[EmployeeId] IS NOT NULL")
            .HasDatabaseName("UX_AppUsers_EmployeeId");

        appUser.HasOne(user => user.Employee)
            .WithOne(employee => employee.AppUser)
            .HasForeignKey<AppUser>(user => user.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

    }

    private static void ConfigureStockItems(ModelBuilder modelBuilder)
    {
        var stockItem = modelBuilder.Entity<StockItem>();

        stockItem.ToTable("StockItems", table =>
        {
            table.HasCheckConstraint(
                "CK_StockItems_CurrentQuantity_NonNegative",
                "[CurrentQuantity] >= 0");
            table.HasCheckConstraint(
                "CK_StockItems_MinimumQuantity_NonNegative",
                "[MinimumQuantity] >= 0");
        });

        stockItem.HasKey(item => item.Id);
        stockItem.Property(item => item.Id).HasMaxLength(64);
        stockItem.Property(item => item.ItemCode).HasMaxLength(50).IsRequired();
        stockItem.Property(item => item.Name).HasMaxLength(150).IsRequired();
        stockItem.Property(item => item.Category).HasMaxLength(100).IsRequired();
        stockItem.Property(item => item.BrandModel).HasMaxLength(150).IsRequired();
        stockItem.Property(item => item.Unit).HasMaxLength(30).IsRequired();
        stockItem.Property(item => item.Location).HasMaxLength(150).IsRequired();

        stockItem.HasIndex(item => item.ItemCode)
            .IsUnique()
            .HasDatabaseName("UX_StockItems_ItemCode");

        IgnoreLegacyDemoData(
            new StockItem
            {
                Id = "stock-item-001",
                ItemCode = "STK-2026-0001",
                Name = "Kablosuz Mouse",
                Category = "Çevre Birimi",
                BrandModel = "Logitech M185",
                Unit = "Adet",
                CurrentQuantity = 35,
                MinimumQuantity = 10,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-002",
                ItemCode = "STK-2026-0002",
                Name = "Kablolu Klavye",
                Category = "Çevre Birimi",
                BrandModel = "Logitech K120",
                Unit = "Adet",
                CurrentQuantity = 18,
                MinimumQuantity = 8,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-003",
                ItemCode = "STK-2026-0003",
                Name = "USB Kulaklık",
                Category = "Çevre Birimi",
                BrandModel = "Jabra Evolve2 30",
                Unit = "Adet",
                CurrentQuantity = 6,
                MinimumQuantity = 5,
                Location = "Ankara Ofis",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-004",
                ItemCode = "STK-2026-0004",
                Name = "HDMI Kablo 2 m",
                Category = "Kablo",
                BrandModel = "Ugreen High Speed",
                Unit = "Adet",
                CurrentQuantity = 12,
                MinimumQuantity = 12,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-005",
                ItemCode = "STK-2026-0005",
                Name = "DisplayPort Kablo 1,8 m",
                Category = "Kablo",
                BrandModel = "Ugreen DP 1.4",
                Unit = "Adet",
                CurrentQuantity = 4,
                MinimumQuantity = 8,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-006",
                ItemCode = "STK-2026-0006",
                Name = "Ethernet Kablo 3 m",
                Category = "Kablo",
                BrandModel = "Digitus Cat6",
                Unit = "Adet",
                CurrentQuantity = 50,
                MinimumQuantity = 20,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-007",
                ItemCode = "STK-2026-0007",
                Name = "USB Bellek 64 GB",
                Category = "Depolama",
                BrandModel = "Kingston DataTraveler Exodia",
                Unit = "Adet",
                CurrentQuantity = 7,
                MinimumQuantity = 10,
                Location = "Ankara Ofis",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-008",
                ItemCode = "STK-2026-0008",
                Name = "Siyah Toner",
                Category = "Sarf Malzeme",
                BrandModel = "HP 59A",
                Unit = "Adet",
                CurrentQuantity = 3,
                MinimumQuantity = 5,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-009",
                ItemCode = "STK-2026-0009",
                Name = "USB-C HDMI Adaptör",
                Category = "Adaptör",
                BrandModel = "Baseus Metal Gleam",
                Unit = "Adet",
                CurrentQuantity = 14,
                MinimumQuantity = 6,
                Location = "İzmir Ofis",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-010",
                ItemCode = "STK-2026-0010",
                Name = "Laptop Şarj Adaptörü",
                Category = "Güç Aksesuarı",
                BrandModel = "Lenovo USB-C 65W",
                Unit = "Adet",
                CurrentQuantity = 5,
                MinimumQuantity = 5,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-011",
                ItemCode = "STK-2026-0011",
                Name = "Laptop Şarj Adaptörü",
                Category = "Güç Aksesuarı",
                BrandModel = "Dell USB-C 65W",
                Unit = "Adet",
                CurrentQuantity = 9,
                MinimumQuantity = 4,
                Location = "Ankara Ofis",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-012",
                ItemCode = "STK-2026-0012",
                Name = "AA Pil",
                Category = "Sarf Malzeme",
                BrandModel = "Duracell Alkalin 10'lu",
                Unit = "Paket",
                CurrentQuantity = 40,
                MinimumQuantity = 20,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-013",
                ItemCode = "STK-2026-0013",
                Name = "SSD 1 TB",
                Category = "Depolama",
                BrandModel = "Samsung 990 EVO Plus",
                Unit = "Adet",
                CurrentQuantity = 6,
                MinimumQuantity = 3,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-014",
                ItemCode = "STK-2026-0014",
                Name = "Web Kamera",
                Category = "Çevre Birimi",
                BrandModel = "Logitech C920",
                Unit = "Adet",
                CurrentQuantity = 3,
                MinimumQuantity = 4,
                Location = "Bursa Şube",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-015",
                ItemCode = "STK-2026-0015",
                Name = "USB-C Kablo 2 m",
                Category = "Kablo",
                BrandModel = "Ugreen 100W",
                Unit = "Adet",
                CurrentQuantity = 22,
                MinimumQuantity = 10,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-016",
                ItemCode = "STK-2026-0016",
                Name = "Akım Korumalı Priz",
                Category = "Güç Aksesuarı",
                BrandModel = "Schneider 6'lı",
                Unit = "Adet",
                CurrentQuantity = 8,
                MinimumQuantity = 5,
                Location = "İzmir Ofis",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-017",
                ItemCode = "STK-2026-0017",
                Name = "Termal Macun",
                Category = "Sarf Malzeme",
                BrandModel = "Arctic MX-6 4 g",
                Unit = "Adet",
                CurrentQuantity = 2,
                MinimumQuantity = 3,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-018",
                ItemCode = "STK-2026-0018",
                Name = "Etiket Şeridi",
                Category = "Sarf Malzeme",
                BrandModel = "Brother TZe-231",
                Unit = "Adet",
                CurrentQuantity = 15,
                MinimumQuantity = 6,
                Location = "Ankara Ofis",
                IsActive = true
            });
    }

    private static void ConfigureStockTransactions(ModelBuilder modelBuilder)
    {
        var stockTransaction = modelBuilder.Entity<StockTransaction>();

        stockTransaction.ToTable("StockTransactions", table =>
            table.HasCheckConstraint(
                "CK_StockTransactions_Quantity_Positive",
                "[Quantity] > 0"));

        stockTransaction.HasKey(transaction => transaction.Id);
        stockTransaction.Property(transaction => transaction.Id).HasMaxLength(64);
        stockTransaction.Property(transaction => transaction.StockItemId).HasMaxLength(64);
        stockTransaction.Property(transaction => transaction.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        stockTransaction.Property(transaction => transaction.TransactionDate)
            .HasColumnType("datetimeoffset");
        stockTransaction.Property(transaction => transaction.PerformedByUserId).HasMaxLength(64).IsRequired();
        stockTransaction.Property(transaction => transaction.RecipientEmployeeId).HasMaxLength(64);
        stockTransaction.Property(transaction => transaction.Note).HasMaxLength(500);

        stockTransaction.HasIndex(transaction => new
            {
                transaction.StockItemId,
                transaction.TransactionDate
            })
            .HasDatabaseName("IX_StockTransactions_StockItemId_TransactionDate");

        stockTransaction.HasOne(transaction => transaction.StockItem)
            .WithMany(item => item.Transactions)
            .HasForeignKey(transaction => transaction.StockItemId)
            .OnDelete(DeleteBehavior.Restrict);
        stockTransaction.HasOne(transaction => transaction.PerformedByUser)
            .WithMany(user => user.StockTransactions)
            .HasForeignKey(transaction => transaction.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        stockTransaction.HasOne(transaction => transaction.RecipientEmployee)
            .WithMany(employee => employee.ReceivedStockTransactions)
            .HasForeignKey(transaction => transaction.RecipientEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureLicenses(ModelBuilder modelBuilder)
    {
        var license = modelBuilder.Entity<License>();

        license.ToTable("Licenses", table =>
        {
            table.HasCheckConstraint(
                "CK_Licenses_TotalSeats_NonNegative",
                "[TotalSeats] >= 0");
            table.HasCheckConstraint(
                "CK_Licenses_UsedSeats_NonNegative",
                "[UsedSeats] >= 0");
            table.HasCheckConstraint(
                "CK_Licenses_UsedSeats_NotGreaterThanTotal",
                "[UsedSeats] <= [TotalSeats]");
            table.HasCheckConstraint(
                "CK_Licenses_ExpirationDate_NotBeforeStartDate",
                "[ExpirationDate] IS NULL OR [ExpirationDate] >= [StartDate]");
        });

        license.HasKey(item => item.Id);
        license.Property(item => item.Id).HasMaxLength(64);
        license.Property(item => item.LicenseCode).HasMaxLength(50).IsRequired();
        license.Property(item => item.ProductName).HasMaxLength(150).IsRequired();
        license.Property(item => item.Vendor).HasMaxLength(100).IsRequired();
        license.Property(item => item.LicenseType).HasMaxLength(100).IsRequired();
        license.Property(item => item.StartDate).HasColumnType("date");
        license.Property(item => item.ExpirationDate).HasColumnType("date");
        license.Property(item => item.Notes).HasMaxLength(1000);

        license.HasIndex(item => item.LicenseCode)
            .IsUnique()
            .HasDatabaseName("UX_Licenses_LicenseCode");

        IgnoreLegacyDemoData(
            new License
            {
                Id = "license-001",
                LicenseCode = "LIC-M365-001",
                ProductName = "Microsoft 365 Business Premium",
                Vendor = "Microsoft",
                LicenseType = "Yıllık Abonelik",
                TotalSeats = 120,
                UsedSeats = 98,
                StartDate = new DateOnly(2026, 1, 1),
                ExpirationDate = new DateOnly(2027, 1, 1),
                IsActive = true,
                Notes = "Kurumsal kullanıcı lisansları"
            },
            new License
            {
                Id = "license-002",
                LicenseCode = "LIC-W11-002",
                ProductName = "Windows 11 Pro",
                Vendor = "Microsoft",
                LicenseType = "Cihaz Bazlı",
                TotalSeats = 80,
                UsedSeats = 74,
                StartDate = new DateOnly(2025, 9, 1),
                ExpirationDate = null,
                IsActive = true,
                Notes = "Süresiz cihaz lisansları"
            },
            new License
            {
                Id = "license-003",
                LicenseCode = "LIC-ACROBAT-003",
                ProductName = "Adobe Acrobat Pro",
                Vendor = "Adobe",
                LicenseType = "Yıllık Abonelik",
                TotalSeats = 35,
                UsedSeats = 31,
                StartDate = new DateOnly(2025, 8, 25),
                ExpirationDate = new DateOnly(2026, 8, 25),
                IsActive = true,
                Notes = "Finans ve hukuk ekipleri"
            },
            new License
            {
                Id = "license-004",
                LicenseCode = "LIC-CC-004",
                ProductName = "Adobe Creative Cloud",
                Vendor = "Adobe",
                LicenseType = "Yıllık Abonelik",
                TotalSeats = 18,
                UsedSeats = 16,
                StartDate = new DateOnly(2025, 9, 5),
                ExpirationDate = new DateOnly(2026, 9, 5),
                IsActive = true,
                Notes = "Tasarım ekibi"
            },
            new License
            {
                Id = "license-005",
                LicenseCode = "LIC-JB-005",
                ProductName = "JetBrains All Products Pack",
                Vendor = "JetBrains",
                LicenseType = "Yıllık Abonelik",
                TotalSeats = 25,
                UsedSeats = 22,
                StartDate = new DateOnly(2025, 7, 31),
                ExpirationDate = new DateOnly(2026, 7, 31),
                IsActive = true,
                Notes = "Yazılım geliştirme ekibi"
            },
            new License
            {
                Id = "license-006",
                LicenseCode = "LIC-VS-006",
                ProductName = "Microsoft Visual Studio Professional",
                Vendor = "Microsoft",
                LicenseType = "Yıllık Abonelik",
                TotalSeats = 30,
                UsedSeats = 28,
                StartDate = new DateOnly(2025, 6, 15),
                ExpirationDate = new DateOnly(2026, 6, 15),
                IsActive = true,
                Notes = "Geliştirme araçları"
            },
            new License
            {
                Id = "license-007",
                LicenseCode = "LIC-CAD-007",
                ProductName = "AutoCAD",
                Vendor = "Autodesk",
                LicenseType = "Eşzamanlı Kullanım",
                TotalSeats = 12,
                UsedSeats = 9,
                StartDate = new DateOnly(2026, 4, 1),
                ExpirationDate = new DateOnly(2027, 4, 1),
                IsActive = true,
                Notes = "Teknik tasarım lisansları"
            },
            new License
            {
                Id = "license-008",
                LicenseCode = "LIC-ESET-008",
                ProductName = "ESET Endpoint Security",
                Vendor = "ESET",
                LicenseType = "Yıllık Abonelik",
                TotalSeats = 350,
                UsedSeats = 324,
                StartDate = new DateOnly(2026, 3, 1),
                ExpirationDate = new DateOnly(2027, 3, 1),
                IsActive = true,
                Notes = "Uç nokta güvenliği"
            },
            new License
            {
                Id = "license-009",
                LicenseCode = "LIC-ZOOM-009",
                ProductName = "Zoom Workplace Business",
                Vendor = "Zoom",
                LicenseType = "Yıllık Abonelik",
                TotalSeats = 45,
                UsedSeats = 12,
                StartDate = new DateOnly(2025, 12, 1),
                ExpirationDate = new DateOnly(2026, 12, 1),
                IsActive = false,
                Notes = "Kullanımdan kaldırılan paket"
            },
            new License
            {
                Id = "license-010",
                LicenseCode = "LIC-POWERBI-010",
                ProductName = "Power BI Pro",
                Vendor = "Microsoft",
                LicenseType = "Aylık Abonelik",
                TotalSeats = 50,
                UsedSeats = 37,
                StartDate = new DateOnly(2026, 8, 1),
                ExpirationDate = new DateOnly(2026, 9, 1),
                IsActive = false,
                Notes = "Geçici olarak pasife alınmış paket"
            });
    }

    private static void ConfigureMaintenance(ModelBuilder modelBuilder)
    {
        var maintenancePlan = modelBuilder.Entity<MaintenancePlan>();

        maintenancePlan.ToTable("MaintenancePlans", table =>
            table.HasCheckConstraint(
                "CK_MaintenancePlans_FrequencyDays_Positive",
                "[FrequencyDays] > 0"));
        maintenancePlan.HasKey(plan => plan.Id);
        maintenancePlan.Property(plan => plan.Id).HasMaxLength(64);
        maintenancePlan.Property(plan => plan.AssetId).HasMaxLength(64).IsRequired();
        maintenancePlan.Property(plan => plan.Name).HasMaxLength(150).IsRequired();
        maintenancePlan.Property(plan => plan.Description).HasMaxLength(1000);
        maintenancePlan.Property(plan => plan.StartDate).HasColumnType("date");
        maintenancePlan.Property(plan => plan.ResponsibleUserId).HasMaxLength(64).IsRequired();
        maintenancePlan.Property(plan => plan.NextDueAt).HasColumnType("date");
        maintenancePlan.Property(plan => plan.CreatedAt).HasColumnType("datetimeoffset");
        maintenancePlan.HasIndex(plan => plan.AssetId)
            .HasDatabaseName("IX_MaintenancePlans_AssetId");
        maintenancePlan.HasOne(plan => plan.Asset)
            .WithMany(asset => asset.MaintenancePlans)
            .HasForeignKey(plan => plan.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
        maintenancePlan.HasOne(plan => plan.ResponsibleUser)
            .WithMany(user => user.ResponsibleMaintenancePlans)
            .HasForeignKey(plan => plan.ResponsibleUserId)
            .OnDelete(DeleteBehavior.Restrict);

        var maintenanceTask = modelBuilder.Entity<MaintenanceTask>();

        maintenanceTask.ToTable("MaintenanceTasks");
        maintenanceTask.HasKey(task => task.Id);
        maintenanceTask.Property(task => task.Id).HasMaxLength(64);
        maintenanceTask.Property(task => task.MaintenancePlanId).HasMaxLength(64).IsRequired();
        maintenanceTask.Property(task => task.AssetId).HasMaxLength(64).IsRequired();
        maintenanceTask.Property(task => task.Title).HasMaxLength(150).IsRequired();
        maintenanceTask.Property(task => task.Description).HasMaxLength(1000);
        maintenanceTask.Property(task => task.PlannedDate).HasColumnType("date");
        maintenanceTask.Property(task => task.CompletedDate).HasColumnType("date");
        maintenanceTask.Property(task => task.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        maintenanceTask.Property(task => task.Notes).HasMaxLength(1000);
        maintenanceTask.Property(task => task.CompletedByUserId).HasMaxLength(64);
        maintenanceTask.Property(task => task.Result).HasMaxLength(1000);
        maintenanceTask.Property(task => task.WorkNotes).HasMaxLength(1000);
        maintenanceTask.Property(task => task.CancellationReason).HasMaxLength(1000);
        maintenanceTask.Property(task => task.CreatedAt).HasColumnType("datetimeoffset");
        maintenanceTask.HasIndex(task => new { task.MaintenancePlanId, task.PlannedDate })
            .IsUnique()
            .HasDatabaseName("IX_MaintenanceTasks_PlanId_PlannedDate");
        maintenanceTask.HasOne(task => task.MaintenancePlan)
            .WithMany(plan => plan.Tasks)
            .HasForeignKey(task => task.MaintenancePlanId)
            .OnDelete(DeleteBehavior.Restrict);
        maintenanceTask.HasOne(task => task.Asset)
            .WithMany(asset => asset.MaintenanceTasks)
            .HasForeignKey(task => task.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
        maintenanceTask.HasOne(task => task.CompletedByUser)
            .WithMany(user => user.CompletedMaintenanceTasks)
            .HasForeignKey(task => task.CompletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        var createdAt = new DateTimeOffset(2026, 8, 1, 9, 0, 0, TimeSpan.Zero);

        IgnoreLegacyDemoData(
            new MaintenancePlan { Id = "maintenance-plan-001", AssetId = "asset-db-001", Name = "Periyodik Donanım Kontrolü", Description = "Genel donanım ve bağlantı kontrolleri.", FrequencyDays = 90, StartDate = new DateOnly(2026, 8, 18), ResponsibleUserId = "app-user-it", EstimatedDurationMinutes = 60, ReminderLeadDays = 7, NextDueAt = new DateOnly(2026, 8, 18), IsActive = true, CreatedAt = createdAt },
            new MaintenancePlan { Id = "maintenance-plan-002", AssetId = "asset-db-002", Name = "Fan ve Soğutma Bakımı", Description = "Fan temizliği ve sıcaklık kontrolü.", FrequencyDays = 120, StartDate = new DateOnly(2026, 9, 15), ResponsibleUserId = "app-user-it", EstimatedDurationMinutes = 45, ReminderLeadDays = 7, NextDueAt = new DateOnly(2026, 9, 15), IsActive = true, CreatedAt = createdAt },
            new MaintenancePlan { Id = "maintenance-plan-003", AssetId = "asset-db-003", Name = "Disk Sağlık Kontrolü", Description = "Disk sağlığı ve performans kontrolü.", FrequencyDays = 60, StartDate = new DateOnly(2026, 8, 1), ResponsibleUserId = "app-user-it", EstimatedDurationMinutes = 30, ReminderLeadDays = 5, NextDueAt = new DateOnly(2026, 8, 1), IsActive = true, CreatedAt = createdAt },
            new MaintenancePlan { Id = "maintenance-plan-004", AssetId = "asset-db-006", Name = "Ekran ve Bağlantı Kontrolü", Description = "Panel, kablo ve girişlerin kontrolü.", FrequencyDays = 180, StartDate = new DateOnly(2026, 10, 10), ResponsibleUserId = "app-user-it", EstimatedDurationMinutes = 30, ReminderLeadDays = 10, NextDueAt = new DateOnly(2026, 10, 10), IsActive = true, CreatedAt = createdAt },
            new MaintenancePlan { Id = "maintenance-plan-005", AssetId = "asset-db-008", Name = "Batarya Sağlık Kontrolü", Description = "Batarya kapasitesi ve şarj döngüsü kontrolü.", FrequencyDays = 90, StartDate = new DateOnly(2026, 8, 20), ResponsibleUserId = "app-user-it", EstimatedDurationMinutes = 30, ReminderLeadDays = 7, NextDueAt = new DateOnly(2026, 8, 20), IsActive = true, CreatedAt = createdAt },
            new MaintenancePlan { Id = "maintenance-plan-006", AssetId = "asset-db-010", Name = "Yazıcı Periyodik Bakımı", Description = "Sarf ve baskı mekanizması kontrolü.", FrequencyDays = 60, StartDate = new DateOnly(2026, 7, 10), ResponsibleUserId = "app-user-it", EstimatedDurationMinutes = 60, ReminderLeadDays = 7, NextDueAt = new DateOnly(2026, 9, 8), IsActive = true, CreatedAt = createdAt });

        IgnoreLegacyDemoData(
            new MaintenanceTask { Id = "maintenance-task-001", MaintenancePlanId = "maintenance-plan-001", AssetId = "asset-db-001", Title = "Periyodik Donanım Kontrolü", Description = "Genel donanım ve bağlantı kontrolleri.", PlannedDate = new DateOnly(2026, 8, 18), Status = MaintenanceTaskStatus.Planned, CreatedAt = createdAt },
            new MaintenanceTask { Id = "maintenance-task-002", MaintenancePlanId = "maintenance-plan-002", AssetId = "asset-db-002", Title = "Fan ve Soğutma Bakımı", Description = "Fan temizliği ve sıcaklık kontrolü.", PlannedDate = new DateOnly(2026, 9, 15), Status = MaintenanceTaskStatus.Planned, CreatedAt = createdAt },
            new MaintenanceTask { Id = "maintenance-task-003", MaintenancePlanId = "maintenance-plan-003", AssetId = "asset-db-003", Title = "Disk Sağlık Kontrolü", Description = "Disk sağlığı ve performans kontrolü.", PlannedDate = new DateOnly(2026, 8, 1), Status = MaintenanceTaskStatus.Planned, Notes = "Öncelikli kontrol edilecek.", CreatedAt = createdAt },
            new MaintenanceTask { Id = "maintenance-task-004", MaintenancePlanId = "maintenance-plan-004", AssetId = "asset-db-006", Title = "Ekran ve Bağlantı Kontrolü", Description = "Panel, kablo ve girişlerin kontrolü.", PlannedDate = new DateOnly(2026, 10, 10), Status = MaintenanceTaskStatus.Planned, CreatedAt = createdAt },
            new MaintenanceTask { Id = "maintenance-task-005", MaintenancePlanId = "maintenance-plan-005", AssetId = "asset-db-008", Title = "Batarya Sağlık Kontrolü", Description = "Batarya kapasitesi ve şarj döngüsü kontrolü.", PlannedDate = new DateOnly(2026, 8, 20), Status = MaintenanceTaskStatus.Planned, CreatedAt = createdAt },
            new MaintenanceTask { Id = "maintenance-task-006", MaintenancePlanId = "maintenance-plan-006", AssetId = "asset-db-010", Title = "Yazıcı Periyodik Bakımı", PlannedDate = new DateOnly(2026, 7, 10), CompletedDate = new DateOnly(2026, 7, 11), Status = MaintenanceTaskStatus.Completed, Notes = "Bakım tamamlandı.", CompletedByUserId = "app-user-it", Result = "Baskı ve besleme kontrolleri başarılı.", WorkNotes = "Temizlik yapıldı ve test çıktısı alındı.", CreatedAt = createdAt },
            new MaintenanceTask { Id = "maintenance-task-007", MaintenancePlanId = "maintenance-plan-001", AssetId = "asset-db-001", Title = "Periyodik Donanım Kontrolü", PlannedDate = new DateOnly(2026, 5, 20), CompletedDate = new DateOnly(2026, 5, 20), Status = MaintenanceTaskStatus.Completed, Notes = "Sorun bulunmadı.", CompletedByUserId = "app-user-it", Result = "Donanım kontrolleri başarılı.", WorkNotes = "Bağlantılar ve sistem bileşenleri kontrol edildi.", CreatedAt = createdAt },
            new MaintenanceTask { Id = "maintenance-task-008", MaintenancePlanId = "maintenance-plan-002", AssetId = "asset-db-002", Title = "Fan ve Soğutma Bakımı", PlannedDate = new DateOnly(2026, 7, 25), Status = MaintenanceTaskStatus.Planned, CreatedAt = createdAt },
            new MaintenanceTask { Id = "maintenance-task-009", MaintenancePlanId = "maintenance-plan-003", AssetId = "asset-db-003", Title = "Disk Sağlık Kontrolü", PlannedDate = new DateOnly(2026, 8, 10), Status = MaintenanceTaskStatus.Cancelled, Notes = "Cihaz kullanımda olduğu için iptal edildi.", CancellationReason = "Cihaz operasyonel kullanımda olduğu için iptal edildi.", CreatedAt = createdAt },
            new MaintenanceTask { Id = "maintenance-task-010", MaintenancePlanId = "maintenance-plan-006", AssetId = "asset-db-010", Title = "Yazıcı Periyodik Bakımı", PlannedDate = new DateOnly(2026, 9, 8), Status = MaintenanceTaskStatus.Planned, CreatedAt = createdAt });

        ConfigureMaintenanceRequests(modelBuilder, createdAt);
    }

    private static void ConfigureMaintenanceRequests(
        ModelBuilder modelBuilder,
        DateTimeOffset createdAt)
    {
        var request = modelBuilder.Entity<MaintenanceRequest>();

        request.ToTable("MaintenanceRequests");
        request.HasKey(item => item.Id);
        request.Property(item => item.Id).HasMaxLength(64);
        request.Property(item => item.AssetId).HasMaxLength(64).IsRequired();
        request.Property(item => item.RequestedByEmployeeId).HasMaxLength(64).IsRequired();
        request.Property(item => item.AssignedToUserId).HasMaxLength(64);
        request.Property(item => item.CompletedByUserId).HasMaxLength(64);
        request.Property(item => item.Title).HasMaxLength(150).IsRequired();
        request.Property(item => item.Description).HasMaxLength(2000).IsRequired();
        request.Property(item => item.Priority).HasConversion<string>().HasMaxLength(30).IsRequired();
        request.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        request.Property(item => item.CreatedAt).HasColumnType("datetimeoffset");
        request.Property(item => item.UpdatedAt).HasColumnType("datetimeoffset");
        request.Property(item => item.CompletedAt).HasColumnType("datetimeoffset");
        request.Property(item => item.Result).HasMaxLength(1000);
        request.Property(item => item.WorkNotes).HasMaxLength(1000);
        request.Property(item => item.CancellationReason).HasMaxLength(1000);
        request.HasIndex(item => new { item.Status, item.Priority, item.CreatedAt })
            .HasDatabaseName("IX_MaintenanceRequests_Status_Priority_CreatedAt");
        request.HasOne(item => item.Asset)
            .WithMany(asset => asset.MaintenanceRequests)
            .HasForeignKey(item => item.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
        request.HasOne(item => item.RequestedByEmployee)
            .WithMany(employee => employee.SupportRequests)
            .HasForeignKey(item => item.RequestedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        request.HasOne(item => item.AssignedToUser)
            .WithMany(user => user.AssignedSupportRequests)
            .HasForeignKey(item => item.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);
        request.HasOne(item => item.CompletedByUser)
            .WithMany(user => user.CompletedSupportRequests)
            .HasForeignKey(item => item.CompletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        IgnoreLegacyDemoData(
            new MaintenanceRequest { Id = "maintenance-request-001", AssetId = "asset-db-001", RequestedByEmployeeId = "employee-db-001", Title = "Cihaz çok ısınıyor", Description = "Yoğun kullanım sırasında fan sürekli yüksek hızda çalışıyor.", Priority = MaintenanceRequestPriority.High, Status = MaintenanceRequestStatus.Open, CreatedAt = createdAt.AddDays(7), UpdatedAt = createdAt.AddDays(7) },
            new MaintenanceRequest { Id = "maintenance-request-002", AssetId = "asset-db-008", RequestedByEmployeeId = "employee-db-002", Title = "Wi-Fi bağlantısı kopuyor", Description = "Kablosuz bağlantı aralıklarla kesiliyor.", Priority = MaintenanceRequestPriority.Normal, Status = MaintenanceRequestStatus.Assigned, AssignedToUserId = "app-user-it", CreatedAt = createdAt.AddDays(8), UpdatedAt = createdAt.AddDays(9) },
            new MaintenanceRequest { Id = "maintenance-request-003", AssetId = "asset-db-001", RequestedByEmployeeId = "employee-db-001", Title = "VPN bağlanmıyor", Description = "Kurumsal VPN bağlantısı kurulamıyor.", Priority = MaintenanceRequestPriority.Critical, Status = MaintenanceRequestStatus.InProgress, AssignedToUserId = "app-user-it", CreatedAt = createdAt.AddDays(9), UpdatedAt = createdAt.AddDays(10) },
            new MaintenanceRequest { Id = "maintenance-request-004", AssetId = "asset-db-008", RequestedByEmployeeId = "employee-db-002", Title = "Uygulama açılmıyor", Description = "Kurumsal uygulama başlatılamıyor.", Priority = MaintenanceRequestPriority.Low, Status = MaintenanceRequestStatus.Completed, AssignedToUserId = "app-user-it", CreatedAt = createdAt.AddDays(2), UpdatedAt = createdAt.AddDays(4), CompletedAt = createdAt.AddDays(4), CompletedByUserId = "app-user-it", Result = "Uygulama yapılandırması yenilendi.", WorkNotes = "Yapılandırma sonrası erişim test edildi." });
    }

    // Bu nesneler eski migration'ların geri alınabilmesi için kaynakta geçici olarak korunur;
    // modele seed olarak eklenmez. Yeni demo verileri yalnız DevelopmentDataSeeder tarafından üretilir.
    private static void IgnoreLegacyDemoData(params object[] records) =>
        _ = records;
}
