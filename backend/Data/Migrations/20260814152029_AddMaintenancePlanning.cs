using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenancePlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenancePlans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssetId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FrequencyDays = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenancePlans", x => x.Id);
                    table.CheckConstraint("CK_MaintenancePlans_FrequencyDays_Positive", "[FrequencyDays] > 0");
                    table.ForeignKey(
                        name: "FK_MaintenancePlans_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceTasks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MaintenancePlanId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssetId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PlannedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CompletedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TechnicianName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_MaintenancePlans_MaintenancePlanId",
                        column: x => x.MaintenancePlanId,
                        principalTable: "MaintenancePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MaintenancePlans",
                columns: new[] { "Id", "AssetId", "CreatedAt", "Description", "FrequencyDays", "IsActive", "Name", "StartDate" },
                values: new object[,]
                {
                    { "maintenance-plan-001", "asset-db-001", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Genel donanım ve bağlantı kontrolleri.", 90, true, "Periyodik Donanım Kontrolü", new DateOnly(2026, 8, 18) },
                    { "maintenance-plan-002", "asset-db-002", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Fan temizliği ve sıcaklık kontrolü.", 120, true, "Fan ve Soğutma Bakımı", new DateOnly(2026, 9, 15) },
                    { "maintenance-plan-003", "asset-db-003", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Disk sağlığı ve performans kontrolü.", 60, true, "Disk Sağlık Kontrolü", new DateOnly(2026, 8, 1) },
                    { "maintenance-plan-004", "asset-db-006", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Panel, kablo ve girişlerin kontrolü.", 180, true, "Ekran ve Bağlantı Kontrolü", new DateOnly(2026, 10, 10) },
                    { "maintenance-plan-005", "asset-db-008", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Batarya kapasitesi ve şarj döngüsü kontrolü.", 90, true, "Batarya Sağlık Kontrolü", new DateOnly(2026, 8, 20) },
                    { "maintenance-plan-006", "asset-db-010", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Sarf ve baskı mekanizması kontrolü.", 60, true, "Yazıcı Periyodik Bakımı", new DateOnly(2026, 7, 10) }
                });

            migrationBuilder.InsertData(
                table: "MaintenanceTasks",
                columns: new[] { "Id", "AssetId", "CompletedDate", "CreatedAt", "Description", "MaintenancePlanId", "Notes", "PlannedDate", "Status", "TechnicianName", "Title" },
                values: new object[,]
                {
                    { "maintenance-task-001", "asset-db-001", null, new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Genel donanım ve bağlantı kontrolleri.", "maintenance-plan-001", null, new DateOnly(2026, 8, 18), "Planned", "Teknik Ekip A", "Periyodik Donanım Kontrolü" },
                    { "maintenance-task-002", "asset-db-002", null, new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Fan temizliği ve sıcaklık kontrolü.", "maintenance-plan-002", null, new DateOnly(2026, 9, 15), "Planned", "Teknik Ekip B", "Fan ve Soğutma Bakımı" },
                    { "maintenance-task-003", "asset-db-003", null, new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Disk sağlığı ve performans kontrolü.", "maintenance-plan-003", "Öncelikli kontrol edilecek.", new DateOnly(2026, 8, 1), "Planned", "Teknik Ekip A", "Disk Sağlık Kontrolü" },
                    { "maintenance-task-004", "asset-db-006", null, new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Panel, kablo ve girişlerin kontrolü.", "maintenance-plan-004", null, new DateOnly(2026, 10, 10), "Planned", null, "Ekran ve Bağlantı Kontrolü" },
                    { "maintenance-task-005", "asset-db-008", null, new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Batarya kapasitesi ve şarj döngüsü kontrolü.", "maintenance-plan-005", null, new DateOnly(2026, 8, 20), "Planned", "Mobil Destek Ekibi", "Batarya Sağlık Kontrolü" },
                    { "maintenance-task-006", "asset-db-010", new DateOnly(2026, 7, 11), new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "maintenance-plan-006", "Bakım tamamlandı.", new DateOnly(2026, 7, 10), "Completed", "Teknik Ekip B", "Yazıcı Periyodik Bakımı" },
                    { "maintenance-task-007", "asset-db-001", new DateOnly(2026, 5, 20), new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "maintenance-plan-001", "Sorun bulunmadı.", new DateOnly(2026, 5, 20), "Completed", "Teknik Ekip A", "Periyodik Donanım Kontrolü" },
                    { "maintenance-task-008", "asset-db-002", null, new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "maintenance-plan-002", null, new DateOnly(2026, 7, 25), "Planned", "Teknik Ekip B", "Fan ve Soğutma Bakımı" },
                    { "maintenance-task-009", "asset-db-003", null, new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "maintenance-plan-003", "Cihaz kullanımda olduğu için iptal edildi.", new DateOnly(2026, 8, 10), "Cancelled", null, "Disk Sağlık Kontrolü" },
                    { "maintenance-task-010", "asset-db-010", null, new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "maintenance-plan-006", null, new DateOnly(2026, 9, 8), "Planned", "Teknik Ekip B", "Yazıcı Periyodik Bakımı" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePlans_AssetId",
                table: "MaintenancePlans",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_AssetId",
                table: "MaintenanceTasks",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_PlanId_PlannedDate",
                table: "MaintenanceTasks",
                columns: new[] { "MaintenancePlanId", "PlannedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceTasks");

            migrationBuilder.DropTable(
                name: "MaintenancePlans");
        }
    }
}
