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
            StockItem => true,
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

        ConfigureMaintenanceRequests(modelBuilder);
    }

    private static void ConfigureMaintenanceRequests(ModelBuilder modelBuilder)
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
    }
}
