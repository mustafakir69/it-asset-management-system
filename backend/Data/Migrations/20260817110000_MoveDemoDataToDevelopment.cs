using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TakipProgrami.Api.Data.Migrations;

public partial class MoveDemoDataToDevelopment : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- DevelopmentDataSeeder daha önce çalıştıysa mevcut development verisini koru.
            -- Production ve temiz kurulumlarda eski model-seed demo kayıtlarını kaldır.
            IF NOT EXISTS (SELECT 1 FROM [Employees] WHERE [Id] = 'employee-db-030')
            BEGIN
                DELETE FROM [MaintenanceNotifications]
                WHERE [MaintenanceTaskId] IN (
                    'maintenance-task-001','maintenance-task-002','maintenance-task-003','maintenance-task-004','maintenance-task-005',
                    'maintenance-task-006','maintenance-task-007','maintenance-task-008','maintenance-task-009','maintenance-task-010');

                DELETE FROM [MaintenanceRequests]
                WHERE [Id] IN (
                    'maintenance-request-001','maintenance-request-002','maintenance-request-003','maintenance-request-004',
                    'maintenance-request-005','maintenance-request-006','maintenance-request-007');

                DELETE FROM [MaintenanceTasks]
                WHERE [Id] IN (
                    'maintenance-task-001','maintenance-task-002','maintenance-task-003','maintenance-task-004','maintenance-task-005',
                    'maintenance-task-006','maintenance-task-007','maintenance-task-008','maintenance-task-009','maintenance-task-010');

                DELETE FROM [MaintenancePlans]
                WHERE [Id] IN (
                    'maintenance-plan-001','maintenance-plan-002','maintenance-plan-003',
                    'maintenance-plan-004','maintenance-plan-005','maintenance-plan-006');

                DELETE FROM [Assignments]
                WHERE [Id] IN (
                    'assignment-db-001','assignment-db-002','assignment-db-003','assignment-db-004',
                    'assignment-db-005','assignment-db-006','assignment-db-007','assignment-db-008');

                DELETE FROM [Licenses]
                WHERE [Id] IN (
                    'license-001','license-002','license-003','license-004','license-005',
                    'license-006','license-007','license-008','license-009','license-010');

                DELETE FROM [StockAlerts]
                WHERE [StockItemId] IN (
                    'stock-item-001','stock-item-002','stock-item-003','stock-item-004','stock-item-005','stock-item-006',
                    'stock-item-007','stock-item-008','stock-item-009','stock-item-010','stock-item-011','stock-item-012',
                    'stock-item-013','stock-item-014','stock-item-015','stock-item-016','stock-item-017','stock-item-018');

                DELETE item FROM [StockItems] item
                WHERE item.[Id] IN (
                    'stock-item-001','stock-item-002','stock-item-003','stock-item-004','stock-item-005','stock-item-006',
                    'stock-item-007','stock-item-008','stock-item-009','stock-item-010','stock-item-011','stock-item-012',
                    'stock-item-013','stock-item-014','stock-item-015','stock-item-016','stock-item-017','stock-item-018')
                  AND NOT EXISTS (SELECT 1 FROM [StockTransactions] movement WHERE movement.[StockItemId] = item.[Id]);

                UPDATE [AppUsers]
                SET [EmployeeId] = NULL
                WHERE [Id] IN ('app-user-employee','app-user-inactive') AND [IsActive] = 0;

                DELETE asset FROM [Assets] asset
                WHERE asset.[Id] IN (
                    'asset-db-001','asset-db-002','asset-db-003','asset-db-004','asset-db-005',
                    'asset-db-006','asset-db-007','asset-db-008','asset-db-009','asset-db-010')
                  AND NOT EXISTS (SELECT 1 FROM [Assignments] value WHERE value.[AssetId] = asset.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [MaintenancePlans] value WHERE value.[AssetId] = asset.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [MaintenanceTasks] value WHERE value.[AssetId] = asset.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [MaintenanceRequests] value WHERE value.[AssetId] = asset.[Id]);

                DELETE employee FROM [Employees] employee
                WHERE employee.[Id] IN (
                    'employee-db-001','employee-db-002','employee-db-003','employee-db-004','employee-db-005',
                    'employee-db-006','employee-db-007','employee-db-008','employee-db-009')
                  AND NOT EXISTS (SELECT 1 FROM [Assignments] value WHERE value.[EmployeeId] = employee.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [MaintenanceRequests] value WHERE value.[RequestedByEmployeeId] = employee.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [StockTransactions] value WHERE value.[RecipientEmployeeId] = employee.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [AppUsers] value WHERE value.[EmployeeId] = employee.[Id]);

                DELETE demoUser FROM [AppUsers] demoUser
                WHERE demoUser.[Id] IN ('app-user-admin','app-user-it','app-user-employee','app-user-inactive')
                  AND demoUser.[IsActive] = 0
                  AND NOT EXISTS (SELECT 1 FROM [Assignments] value WHERE value.[AssignedByUserId] = demoUser.[Id] OR value.[ReturnedByUserId] = demoUser.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [StockTransactions] value WHERE value.[PerformedByUserId] = demoUser.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [MaintenancePlans] value WHERE value.[ResponsibleUserId] = demoUser.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [MaintenanceTasks] value WHERE value.[CompletedByUserId] = demoUser.[Id])
                  AND NOT EXISTS (SELECT 1 FROM [MaintenanceRequests] value WHERE value.[AssignedToUserId] = demoUser.[Id] OR value.[CompletedByUserId] = demoUser.[Id]);
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Production'a demo veri geri eklenmez.
    }
}
