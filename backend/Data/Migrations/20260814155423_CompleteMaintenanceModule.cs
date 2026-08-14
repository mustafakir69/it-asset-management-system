using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompleteMaintenanceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaintenanceTasks_PlanId_PlannedDate",
                table: "MaintenanceTasks");

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "MaintenanceTasks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                table: "MaintenanceTasks",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "MaintenanceTasks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkNotes",
                table: "MaintenanceTasks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleTechnician",
                table: "MaintenancePlans",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "Atanmamış");

            migrationBuilder.CreateTable(
                name: "MaintenanceRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssetId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AssignedTechnician = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WorkNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRequests_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.InsertData(
                table: "MaintenanceRequests",
                columns: new[] { "Id", "AssetId", "AssignedTechnician", "CancellationReason", "CompletedAt", "CompletedBy", "CreatedAt", "Description", "Priority", "RequestedBy", "Result", "Status", "Title", "UpdatedAt", "WorkNotes" },
                values: new object[,]
                {
                    { "maintenance-request-001", "asset-db-001", null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 8, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Yoğun kullanım sırasında fan sürekli yüksek hızda çalışıyor.", "High", "Operasyon Kullanıcısı", null, "Open", "Cihaz çok ısınıyor", new DateTimeOffset(new DateTime(2026, 8, 8, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "maintenance-request-002", "asset-db-010", "Teknik Ekip B", null, null, null, new DateTimeOffset(new DateTime(2026, 8, 9, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Renkli çıktılarda dikey çizgiler oluşuyor.", "Normal", "Finans Kullanıcısı", null, "Assigned", "Yazıcı çizgili basıyor", new DateTimeOffset(new DateTime(2026, 8, 10, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "maintenance-request-003", "asset-db-006", "Teknik Ekip A", null, null, null, new DateTimeOffset(new DateTime(2026, 8, 10, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Monitör görüntüsü kısa sürelerle kesiliyor.", "Critical", "Tasarım Kullanıcısı", null, "InProgress", "Görüntü aralıklı kesiliyor", new DateTimeOffset(new DateTime(2026, 8, 11, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "maintenance-request-004", "asset-db-002", "Teknik Ekip B", null, new DateTimeOffset(new DateTime(2026, 8, 5, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Teknik Ekip B", new DateTimeOffset(new DateTime(2026, 8, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Sol taraftaki USB bağlantı noktası cihazları algılamıyor.", "Low", "Satış Kullanıcısı", "Bağlantı sürücüsü yenilendi.", "Completed", "USB bağlantısı çalışmıyor", new DateTimeOffset(new DateTime(2026, 8, 5, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Sürücü kurulumu sonrası bağlantı test edildi." },
                    { "maintenance-request-005", "asset-db-008", null, "Cihaz garanti servisine gönderildi.", null, null, new DateTimeOffset(new DateTime(2026, 8, 4, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Batarya normalden hızlı tükeniyor.", "High", "Saha Kullanıcısı", null, "Cancelled", "Şarj süresi çok kısa", new DateTimeOffset(new DateTime(2026, 8, 6, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "maintenance-request-006", "asset-db-004", null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "İlk açılışta disk denetim uyarısı gösteriliyor.", "Critical", "Muhasebe Kullanıcısı", null, "Open", "Cihaz açılışta hata veriyor", new DateTimeOffset(new DateTime(2026, 8, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { "maintenance-request-007", "asset-db-003", "Teknik Ekip A", null, new DateTimeOffset(new DateTime(2026, 8, 7, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Teknik Ekip A", new DateTimeOffset(new DateTime(2026, 8, 2, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Enter tuşu zaman zaman takılı kalıyor.", "Normal", "Destek Kullanıcısı", "Klavye temizliği ve mekanik kontrol tamamlandı.", "Completed", "Klavye tuşu takılıyor", new DateTimeOffset(new DateTime(2026, 8, 7, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tuş mekanizması temizlenerek tekrar test edildi." }
                });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-001",
                columns: new[] { "CancellationReason", "CompletedBy", "Result", "WorkNotes" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-002",
                columns: new[] { "CancellationReason", "CompletedBy", "Result", "WorkNotes" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-003",
                columns: new[] { "CancellationReason", "CompletedBy", "Result", "WorkNotes" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-004",
                columns: new[] { "CancellationReason", "CompletedBy", "Result", "WorkNotes" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-005",
                columns: new[] { "CancellationReason", "CompletedBy", "Result", "WorkNotes" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-006",
                columns: new[] { "CancellationReason", "CompletedBy", "Result", "WorkNotes" },
                values: new object[] { null, "Teknik Ekip B", "Baskı ve besleme kontrolleri başarılı.", "Temizlik yapıldı ve test çıktısı alındı." });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-007",
                columns: new[] { "CancellationReason", "CompletedBy", "Result", "WorkNotes" },
                values: new object[] { null, "Teknik Ekip A", "Donanım kontrolleri başarılı.", "Bağlantılar ve sistem bileşenleri kontrol edildi." });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-008",
                columns: new[] { "CancellationReason", "CompletedBy", "Result", "WorkNotes" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-009",
                columns: new[] { "CancellationReason", "CompletedBy", "Result", "WorkNotes" },
                values: new object[] { "Cihaz operasyonel kullanımda olduğu için iptal edildi.", null, null, null });

            migrationBuilder.UpdateData(
                table: "MaintenanceTasks",
                keyColumn: "Id",
                keyValue: "maintenance-task-010",
                columns: new[] { "CancellationReason", "CompletedBy", "Result", "WorkNotes" },
                values: new object[] { null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_PlanId_PlannedDate",
                table: "MaintenanceTasks",
                columns: new[] { "MaintenancePlanId", "PlannedDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_AssetId",
                table: "MaintenanceRequests",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_Status_Priority_CreatedAt",
                table: "MaintenanceRequests",
                columns: new[] { "Status", "Priority", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceTasks_PlanId_PlannedDate",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "WorkNotes",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "ResponsibleTechnician",
                table: "MaintenancePlans");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_PlanId_PlannedDate",
                table: "MaintenanceTasks",
                columns: new[] { "MaintenancePlanId", "PlannedDate" });
        }
    }
}
