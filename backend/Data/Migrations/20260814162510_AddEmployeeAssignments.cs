using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EmployeeNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssetId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReturnedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AssignedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReturnedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReturnNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.CheckConstraint("CK_Assignments_ReturnedAt_NotBeforeAssignedAt", "[ReturnedAt] IS NULL OR [ReturnedAt] >= [AssignedAt]");
                    table.ForeignKey(
                        name: "FK_Assignments_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "CreatedAt", "Department", "Email", "EmployeeNo", "FullName", "IsActive" },
                values: new object[,]
                {
                    { "employee-db-001", new DateTimeOffset(new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT", "demo.kullanici1@example.test", "EMP-001", "Demo Kullanıcı 1", true },
                    { "employee-db-002", new DateTimeOffset(new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Finans", "demo.kullanici2@example.test", "EMP-002", "Demo Kullanıcı 2", true },
                    { "employee-db-003", new DateTimeOffset(new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "İnsan Kaynakları", "demo.kullanici3@example.test", "EMP-003", "Demo Kullanıcı 3", true },
                    { "employee-db-004", new DateTimeOffset(new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Operasyon", "demo.kullanici4@example.test", "EMP-004", "Demo Kullanıcı 4", true },
                    { "employee-db-005", new DateTimeOffset(new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Satış", "demo.kullanici5@example.test", "EMP-005", "Demo Kullanıcı 5", true },
                    { "employee-db-006", new DateTimeOffset(new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Muhasebe", "demo.kullanici6@example.test", "EMP-006", "Demo Kullanıcı 6", true },
                    { "employee-db-007", new DateTimeOffset(new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Pazarlama", "demo.kullanici7@example.test", "EMP-007", "Demo Kullanıcı 7", true },
                    { "employee-db-008", new DateTimeOffset(new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Lojistik", "demo.kullanici8@example.test", "EMP-008", "Demo Kullanıcı 8", true },
                    { "employee-db-009", new DateTimeOffset(new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Operasyon", "demo.kullanici9@example.test", "EMP-009", "Demo Kullanıcı 9", false }
                });

            migrationBuilder.InsertData(
                table: "Assignments",
                columns: new[] { "Id", "AssetId", "AssignedAt", "AssignedBy", "CreatedAt", "EmployeeId", "Notes", "ReturnNotes", "ReturnedAt", "ReturnedBy" },
                values: new object[,]
                {
                    { "assignment-db-001", "asset-db-001", new DateTimeOffset(new DateTime(2026, 3, 2, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Operasyon", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee-db-001", "Standart çalışma cihazı zimmeti.", null, null, null },
                    { "assignment-db-002", "asset-db-008", new DateTimeOffset(new DateTime(2026, 4, 6, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Operasyon", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee-db-002", "Kurumsal telefon zimmeti.", null, null, null },
                    { "assignment-db-003", "asset-db-002", new DateTimeOffset(new DateTime(2026, 2, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Operasyon", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee-db-003", "Geçici proje cihazı.", "Eksiksiz teslim alındı.", new DateTimeOffset(new DateTime(2026, 5, 15, 16, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Destek" },
                    { "assignment-db-004", "asset-db-004", new DateTimeOffset(new DateTime(2026, 3, 20, 8, 45, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Operasyon", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee-db-004", "Operasyon masası kullanımı.", "Cihaz depoya alındı.", new DateTimeOffset(new DateTime(2026, 6, 30, 17, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Destek" },
                    { "assignment-db-005", "asset-db-006", new DateTimeOffset(new DateTime(2026, 4, 12, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Operasyon", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee-db-005", "Ek çalışma ekranı.", "Fiziksel kontrol tamamlandı.", new DateTimeOffset(new DateTime(2026, 7, 18, 15, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Destek" },
                    { "assignment-db-006", "asset-db-010", new DateTimeOffset(new DateTime(2026, 5, 8, 11, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Operasyon", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee-db-006", "Birim yazıcısı kullanımı.", "Çalışır durumda teslim alındı.", new DateTimeOffset(new DateTime(2026, 7, 22, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Destek" },
                    { "assignment-db-007", "asset-db-003", new DateTimeOffset(new DateTime(2025, 11, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Operasyon", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee-db-007", "Dönemsel kullanım.", "Bakım kontrolü için teslim alındı.", new DateTimeOffset(new DateTime(2026, 1, 9, 16, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Destek" },
                    { "assignment-db-008", "asset-db-007", new DateTimeOffset(new DateTime(2026, 1, 12, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Operasyon", new DateTimeOffset(new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "employee-db-008", "Geçici monitör kullanımı.", "Teslim kaydı tamamlandı.", new DateTimeOffset(new DateTime(2026, 2, 20, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "BT Destek" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_EmployeeId",
                table: "Assignments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "UX_Assignments_AssetId_Active",
                table: "Assignments",
                column: "AssetId",
                unique: true,
                filter: "[ReturnedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Employees_EmployeeNo",
                table: "Employees",
                column: "EmployeeNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
