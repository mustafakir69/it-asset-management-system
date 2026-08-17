using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompanyReadyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: "app-user-auditor");

            migrationBuilder.DropColumn(
                name: "PersonName",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "TechnicianName",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "AssignedTechnician",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "RequestedBy",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "ResponsibleTechnician",
                table: "MaintenancePlans");

            migrationBuilder.DropColumn(
                name: "AssignedBy",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "ReturnedBy",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Employees",
                newName: "CorporateEmail");

            migrationBuilder.AddColumn<string>(
                name: "PerformedByUserId",
                table: "StockTransactions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientEmployeeId",
                table: "StockTransactions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedByUserId",
                table: "MaintenanceTasks",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserId",
                table: "MaintenanceRequests",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedByUserId",
                table: "MaintenanceRequests",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedByEmployeeId",
                table: "MaintenanceRequests",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "MaintenancePlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NextDueAt",
                table: "MaintenancePlans",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "ReminderLeadDays",
                table: "MaintenancePlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleUserId",
                table: "MaintenancePlans",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AssignedByUserId",
                table: "Assignments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReturnedByUserId",
                table: "Assignments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "WarrantyEndDate",
                table: "Assets",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-001",
                columns: new[] { "AssignedByUserId", "ReturnedByUserId" },
                values: new object[] { "app-user-it", null });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-002",
                columns: new[] { "AssignedByUserId", "ReturnedByUserId" },
                values: new object[] { "app-user-it", null });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-003",
                columns: new[] { "AssignedByUserId", "ReturnedByUserId" },
                values: new object[] { "app-user-it", "app-user-it" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-004",
                columns: new[] { "AssignedByUserId", "ReturnedByUserId" },
                values: new object[] { "app-user-it", "app-user-it" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-005",
                columns: new[] { "AssignedByUserId", "ReturnedByUserId" },
                values: new object[] { "app-user-it", "app-user-it" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-006",
                columns: new[] { "AssignedByUserId", "ReturnedByUserId" },
                values: new object[] { "app-user-it", "app-user-it" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-007",
                columns: new[] { "AssignedByUserId", "ReturnedByUserId" },
                values: new object[] { "app-user-it", "app-user-it" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-008",
                columns: new[] { "AssignedByUserId", "ReturnedByUserId" },
                values: new object[] { "app-user-it", "app-user-it" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: "employee-db-001",
                column: "Department",
                value: "Bilgi İşlem");

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-001",
                columns: new[] { "EstimatedDurationMinutes", "NextDueAt", "ReminderLeadDays", "ResponsibleUserId" },
                values: new object[] { 60, new DateOnly(2026, 8, 18), 7, "app-user-it" });

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-002",
                columns: new[] { "EstimatedDurationMinutes", "NextDueAt", "ReminderLeadDays", "ResponsibleUserId" },
                values: new object[] { 45, new DateOnly(2026, 9, 15), 7, "app-user-it" });

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-003",
                columns: new[] { "EstimatedDurationMinutes", "NextDueAt", "ReminderLeadDays", "ResponsibleUserId" },
                values: new object[] { 30, new DateOnly(2026, 8, 1), 5, "app-user-it" });

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-004",
                columns: new[] { "EstimatedDurationMinutes", "NextDueAt", "ReminderLeadDays", "ResponsibleUserId" },
                values: new object[] { 30, new DateOnly(2026, 10, 10), 10, "app-user-it" });

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-005",
                columns: new[] { "EstimatedDurationMinutes", "NextDueAt", "ReminderLeadDays", "ResponsibleUserId" },
                values: new object[] { 30, new DateOnly(2026, 8, 20), 7, "app-user-it" });

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-006",
                columns: new[] { "EstimatedDurationMinutes", "NextDueAt", "ReminderLeadDays", "ResponsibleUserId" },
                values: new object[] { 60, new DateOnly(2026, 9, 8), 7, "app-user-it" });

            migrationBuilder.UpdateData(
                table: "MaintenanceRequests",
                keyColumn: "Id",
                keyValue: "maintenance-request-001",
                columns: new[] { "AssignedToUserId", "CompletedByUserId", "RequestedByEmployeeId" },
                values: new object[] { null, null, "employee-db-001" });

            migrationBuilder.UpdateData(
                table: "MaintenanceRequests",
                keyColumn: "Id",
                keyValue: "maintenance-request-002",
                columns: new[] { "AssetId", "AssignedToUserId", "CompletedByUserId", "Description", "RequestedByEmployeeId", "Title" },
                values: new object[] { "asset-db-008", "app-user-it", null, "Kablosuz bağlantı aralıklarla kesiliyor.", "employee-db-002", "Wi-Fi bağlantısı kopuyor" });

            migrationBuilder.UpdateData(
                table: "MaintenanceRequests",
                keyColumn: "Id",
                keyValue: "maintenance-request-003",
                columns: new[] { "AssetId", "AssignedToUserId", "CompletedByUserId", "Description", "RequestedByEmployeeId", "Title" },
                values: new object[] { "asset-db-001", "app-user-it", null, "Kurumsal VPN bağlantısı kurulamıyor.", "employee-db-001", "VPN bağlanmıyor" });

            migrationBuilder.UpdateData(
                table: "MaintenanceRequests",
                keyColumn: "Id",
                keyValue: "maintenance-request-004",
                columns: new[] { "AssetId", "AssignedToUserId", "CompletedByUserId", "Description", "RequestedByEmployeeId", "Result", "Title", "WorkNotes" },
                values: new object[] { "asset-db-008", "app-user-it", "app-user-it", "Kurumsal uygulama başlatılamıyor.", "employee-db-002", "Uygulama yapılandırması yenilendi.", "Uygulama açılmıyor", "Yapılandırma sonrası erişim test edildi." });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-001",
                column: "CompletedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-002",
                column: "CompletedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-003",
                column: "CompletedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-004",
                column: "CompletedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-005",
                column: "CompletedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-006",
                column: "CompletedByUserId",
                value: "app-user-it");

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-007",
                column: "CompletedByUserId",
                value: "app-user-it");

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-008",
                column: "CompletedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-009",
                column: "CompletedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-010",
                column: "CompletedByUserId",
                value: null);

            // Legacy display-name fields are intentionally mapped to real relational identities
            // before constraints are added. Historical actor text is not trusted as identity.
            migrationBuilder.Sql("""
                UPDATE StockTransactions SET PerformedByUserId = 'app-user-it' WHERE PerformedByUserId = '';
                UPDATE Assignments SET AssignedByUserId = 'app-user-it' WHERE AssignedByUserId = '';
                UPDATE Assignments SET ReturnedByUserId = 'app-user-it' WHERE ReturnedAt IS NOT NULL AND ReturnedByUserId IS NULL;
                UPDATE MaintenancePlans SET ResponsibleUserId = 'app-user-it', EstimatedDurationMinutes = CASE WHEN EstimatedDurationMinutes = 0 THEN 60 ELSE EstimatedDurationMinutes END, ReminderLeadDays = CASE WHEN ReminderLeadDays = 0 THEN 7 ELSE ReminderLeadDays END, NextDueAt = CASE WHEN NextDueAt = '0001-01-01' THEN StartDate ELSE NextDueAt END WHERE ResponsibleUserId = '';
                UPDATE MaintenanceTasks SET CompletedByUserId = 'app-user-it' WHERE CompletedDate IS NOT NULL AND CompletedByUserId IS NULL;
                UPDATE MaintenanceRequests SET RequestedByEmployeeId = CASE WHEN AssetId = 'asset-db-008' THEN 'employee-db-002' ELSE 'employee-db-001' END WHERE RequestedByEmployeeId = '';
                UPDATE MaintenanceRequests SET AssignedToUserId = 'app-user-it' WHERE Status IN ('Assigned','InProgress','Completed') AND AssignedToUserId IS NULL;
                UPDATE MaintenanceRequests SET CompletedByUserId = 'app-user-it' WHERE Status = 'Completed' AND CompletedByUserId IS NULL;
                UPDATE MaintenanceRequests SET AssetId = CASE WHEN RequestedByEmployeeId = 'employee-db-002' THEN 'asset-db-008' ELSE 'asset-db-001' END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_PerformedByUserId",
                table: "StockTransactions",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_RecipientEmployeeId",
                table: "StockTransactions",
                column: "RecipientEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_CompletedByUserId",
                table: "MaintenanceTasks",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_AssignedToUserId",
                table: "MaintenanceRequests",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_CompletedByUserId",
                table: "MaintenanceRequests",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_RequestedByEmployeeId",
                table: "MaintenanceRequests",
                column: "RequestedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePlans_ResponsibleUserId",
                table: "MaintenancePlans",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_AssignedByUserId",
                table: "Assignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ReturnedByUserId",
                table: "Assignments",
                column: "ReturnedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_AppUsers_AssignedByUserId",
                table: "Assignments",
                column: "AssignedByUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_AppUsers_ReturnedByUserId",
                table: "Assignments",
                column: "ReturnedByUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenancePlans_AppUsers_ResponsibleUserId",
                table: "MaintenancePlans",
                column: "ResponsibleUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_AppUsers_AssignedToUserId",
                table: "MaintenanceRequests",
                column: "AssignedToUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_AppUsers_CompletedByUserId",
                table: "MaintenanceRequests",
                column: "CompletedByUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Employees_RequestedByEmployeeId",
                table: "MaintenanceRequests",
                column: "RequestedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceTasks_AppUsers_CompletedByUserId",
                table: "MaintenanceTasks",
                column: "CompletedByUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_AppUsers_PerformedByUserId",
                table: "StockTransactions",
                column: "PerformedByUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Employees_RecipientEmployeeId",
                table: "StockTransactions",
                column: "RecipientEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_AppUsers_AssignedByUserId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_AppUsers_ReturnedByUserId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenancePlans_AppUsers_ResponsibleUserId",
                table: "MaintenancePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_AppUsers_AssignedToUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_AppUsers_CompletedByUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Employees_RequestedByEmployeeId",
                table: "MaintenanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceTasks_AppUsers_CompletedByUserId",
                table: "MaintenanceTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_AppUsers_PerformedByUserId",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Employees_RecipientEmployeeId",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_StockTransactions_PerformedByUserId",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_StockTransactions_RecipientEmployeeId",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceTasks_CompletedByUserId",
                table: "MaintenanceTasks");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_AssignedToUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_CompletedByUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_RequestedByEmployeeId",
                table: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_MaintenancePlans_ResponsibleUserId",
                table: "MaintenancePlans");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_AssignedByUserId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_ReturnedByUserId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "PerformedByUserId",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "RecipientEmployeeId",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "RequestedByEmployeeId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "MaintenancePlans");

            migrationBuilder.DropColumn(
                name: "NextDueAt",
                table: "MaintenancePlans");

            migrationBuilder.DropColumn(
                name: "ReminderLeadDays",
                table: "MaintenancePlans");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserId",
                table: "MaintenancePlans");

            migrationBuilder.DropColumn(
                name: "AssignedByUserId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "ReturnedByUserId",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "CorporateEmail",
                table: "Employees",
                newName: "Email");

            migrationBuilder.AddColumn<string>(
                name: "PersonName",
                table: "StockTransactions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                table: "MaintenanceTasks",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicianName",
                table: "MaintenanceTasks",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedTechnician",
                table: "MaintenanceRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                table: "MaintenanceRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedBy",
                table: "MaintenanceRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleTechnician",
                table: "MaintenancePlans",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "Atanmamış");

            migrationBuilder.AddColumn<string>(
                name: "AssignedBy",
                table: "Assignments",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReturnedBy",
                table: "Assignments",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "WarrantyEndDate",
                table: "Assets",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-001",
                columns: new[] { "AssignedBy", "ReturnedBy" },
                values: new object[] { "BT Operasyon", null });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-002",
                columns: new[] { "AssignedBy", "ReturnedBy" },
                values: new object[] { "BT Operasyon", null });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-003",
                columns: new[] { "AssignedBy", "ReturnedBy" },
                values: new object[] { "BT Operasyon", "BT Destek" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-004",
                columns: new[] { "AssignedBy", "ReturnedBy" },
                values: new object[] { "BT Operasyon", "BT Destek" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-005",
                columns: new[] { "AssignedBy", "ReturnedBy" },
                values: new object[] { "BT Operasyon", "BT Destek" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-006",
                columns: new[] { "AssignedBy", "ReturnedBy" },
                values: new object[] { "BT Operasyon", "BT Destek" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-007",
                columns: new[] { "AssignedBy", "ReturnedBy" },
                values: new object[] { "BT Operasyon", "BT Destek" });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: "assignment-db-008",
                columns: new[] { "AssignedBy", "ReturnedBy" },
                values: new object[] { "BT Operasyon", "BT Destek" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: "employee-db-001",
                column: "Department",
                value: "BT");

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-001",
                column: "ResponsibleTechnician",
                value: "Teknik Ekip A");

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-002",
                column: "ResponsibleTechnician",
                value: "Teknik Ekip B");

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-003",
                column: "ResponsibleTechnician",
                value: "Teknik Ekip A");

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-004",
                column: "ResponsibleTechnician",
                value: "Teknik Ekip B");

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-005",
                column: "ResponsibleTechnician",
                value: "Mobil Destek Ekibi");

            migrationBuilder.UpdateData(
                table: "MaintenancePlans",
                keyColumn: "Id",
                keyValue: "maintenance-plan-006",
                column: "ResponsibleTechnician",
                value: "Teknik Ekip B");

            migrationBuilder.UpdateData(
                table: "MaintenanceRequests",
                keyColumn: "Id",
                keyValue: "maintenance-request-001",
                columns: new[] { "AssignedTechnician", "CompletedBy", "RequestedBy" },
                values: new object[] { null, null, "Operasyon Kullanıcısı" });

            migrationBuilder.UpdateData(
                table: "MaintenanceRequests",
                keyColumn: "Id",
                keyValue: "maintenance-request-002",
                columns: new[] { "AssetId", "AssignedTechnician", "CompletedBy", "Description", "RequestedBy", "Title" },
                values: new object[] { "asset-db-010", "Teknik Ekip B", null, "Renkli çıktılarda dikey çizgiler oluşuyor.", "Finans Kullanıcısı", "Yazıcı çizgili basıyor" });

            migrationBuilder.UpdateData(
                table: "MaintenanceRequests",
                keyColumn: "Id",
                keyValue: "maintenance-request-003",
                columns: new[] { "AssetId", "AssignedTechnician", "CompletedBy", "Description", "RequestedBy", "Title" },
                values: new object[] { "asset-db-006", "Teknik Ekip A", null, "Monitör görüntüsü kısa sürelerle kesiliyor.", "Tasarım Kullanıcısı", "Görüntü aralıklı kesiliyor" });

            migrationBuilder.UpdateData(
                table: "MaintenanceRequests",
                keyColumn: "Id",
                keyValue: "maintenance-request-004",
                columns: new[] { "AssetId", "AssignedTechnician", "CompletedBy", "Description", "RequestedBy", "Result", "Title", "WorkNotes" },
                values: new object[] { "asset-db-002", "Teknik Ekip B", "Teknik Ekip B", "Sol taraftaki USB bağlantı noktası cihazları algılamıyor.", "Satış Kullanıcısı", "Bağlantı sürücüsü yenilendi.", "USB bağlantısı çalışmıyor", "Sürücü kurulumu sonrası bağlantı test edildi." });

            migrationBuilder.InsertData(
                table: "MaintenanceRequests",
                columns: new[] { "Id", "AssetId", "AssignedTechnician", "CancellationReason", "CompletedAt", "CompletedBy", "CreatedAt", "Description", "Priority", "RequestedBy", "Result", "Status", "Title", "UpdatedAt", "WorkNotes" },
                values: new object[,]
                {
                    { "maintenance-request-005", "asset-db-008", null, "Cihaz garanti servisine gönderildi.", null, null, new DateTimeOffset(new DateTime(2026, 8, 4, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Batarya normalden hızlı tükeniyor.", "High", "Saha Kullanıcısı", null, "Cancelled", "Şarj süresi çok kısa", new DateTimeOffset(new DateTime(2026, 8, 6, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "maintenance-request-006", "asset-db-004", null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "İlk açılışta disk denetim uyarısı gösteriliyor.", "Critical", "Muhasebe Kullanıcısı", null, "Open", "Cihaz açılışta hata veriyor", new DateTimeOffset(new DateTime(2026, 8, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "maintenance-request-007", "asset-db-003", "Teknik Ekip A", null, new DateTimeOffset(new DateTime(2026, 8, 7, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Teknik Ekip A", new DateTimeOffset(new DateTime(2026, 8, 2, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Enter tuşu zaman zaman takılı kalıyor.", "Normal", "Destek Kullanıcısı", "Klavye temizliği ve mekanik kontrol tamamlandı.", "Completed", "Klavye tuşu takılıyor", new DateTimeOffset(new DateTime(2026, 8, 7, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tuş mekanizması temizlenerek tekrar test edildi." }
                });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-001",
                columns: new[] { "CompletedBy", "TechnicianName" },
                values: new object[] { null, "Teknik Ekip A" });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-002",
                columns: new[] { "CompletedBy", "TechnicianName" },
                values: new object[] { null, "Teknik Ekip B" });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-003",
                columns: new[] { "CompletedBy", "TechnicianName" },
                values: new object[] { null, "Teknik Ekip A" });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-004",
                columns: new[] { "CompletedBy", "TechnicianName" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-005",
                columns: new[] { "CompletedBy", "TechnicianName" },
                values: new object[] { null, "Mobil Destek Ekibi" });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-006",
                columns: new[] { "CompletedBy", "TechnicianName" },
                values: new object[] { "Teknik Ekip B", "Teknik Ekip B" });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-007",
                columns: new[] { "CompletedBy", "TechnicianName" },
                values: new object[] { "Teknik Ekip A", "Teknik Ekip A" });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-008",
                columns: new[] { "CompletedBy", "TechnicianName" },
                values: new object[] { null, "Teknik Ekip B" });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-009",
                columns: new[] { "CompletedBy", "TechnicianName" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-010",
                columns: new[] { "CompletedBy", "TechnicianName" },
                values: new object[] { null, "Teknik Ekip B" });
        }
    }
}
