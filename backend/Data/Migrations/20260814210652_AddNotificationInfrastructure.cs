using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenanceNotifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MaintenanceTaskId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceNotifications_MaintenanceTasks_MaintenanceTaskId",
                        column: x => x.MaintenanceTaskId,
                        principalTable: "MaintenanceTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockAlerts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StockItemId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TriggeredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    QuantityAtTrigger = table.Column<int>(type: "int", nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAlerts_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceNotifications_ScheduledAt",
                table: "MaintenanceNotifications",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "UX_MaintenanceNotifications_Task_Type",
                table: "MaintenanceNotifications",
                columns: new[] { "MaintenanceTaskId", "NotificationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_TriggeredAt",
                table: "StockAlerts",
                column: "TriggeredAt");

            migrationBuilder.CreateIndex(
                name: "UX_StockAlerts_StockItemId_Active",
                table: "StockAlerts",
                column: "StockItemId",
                unique: true,
                filter: "[ResolvedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceNotifications");

            migrationBuilder.DropTable(
                name: "StockAlerts");
        }
    }
}
