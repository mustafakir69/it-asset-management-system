using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LicenseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LicenseType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalSeats = table.Column<int>(type: "int", nullable: false),
                    UsedSeats = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.Id);
                    table.CheckConstraint("CK_Licenses_ExpirationDate_NotBeforeStartDate", "[ExpirationDate] IS NULL OR [ExpirationDate] >= [StartDate]");
                    table.CheckConstraint("CK_Licenses_TotalSeats_NonNegative", "[TotalSeats] >= 0");
                    table.CheckConstraint("CK_Licenses_UsedSeats_NonNegative", "[UsedSeats] >= 0");
                    table.CheckConstraint("CK_Licenses_UsedSeats_NotGreaterThanTotal", "[UsedSeats] <= [TotalSeats]");
                });

            migrationBuilder.InsertData(
                table: "Licenses",
                columns: new[] { "Id", "ExpirationDate", "IsActive", "LicenseCode", "LicenseType", "Notes", "ProductName", "StartDate", "TotalSeats", "UsedSeats", "Vendor" },
                values: new object[,]
                {
                    { "license-001", new DateOnly(2027, 1, 1), true, "LIC-M365-001", "Yıllık Abonelik", "Kurumsal kullanıcı lisansları", "Microsoft 365 Business Premium", new DateOnly(2026, 1, 1), 120, 98, "Microsoft" },
                    { "license-002", null, true, "LIC-W11-002", "Cihaz Bazlı", "Süresiz cihaz lisansları", "Windows 11 Pro", new DateOnly(2025, 9, 1), 80, 74, "Microsoft" },
                    { "license-003", new DateOnly(2026, 8, 25), true, "LIC-ACROBAT-003", "Yıllık Abonelik", "Finans ve hukuk ekipleri", "Adobe Acrobat Pro", new DateOnly(2025, 8, 25), 35, 31, "Adobe" },
                    { "license-004", new DateOnly(2026, 9, 5), true, "LIC-CC-004", "Yıllık Abonelik", "Tasarım ekibi", "Adobe Creative Cloud", new DateOnly(2025, 9, 5), 18, 16, "Adobe" },
                    { "license-005", new DateOnly(2026, 7, 31), true, "LIC-JB-005", "Yıllık Abonelik", "Yazılım geliştirme ekibi", "JetBrains All Products Pack", new DateOnly(2025, 7, 31), 25, 22, "JetBrains" },
                    { "license-006", new DateOnly(2026, 6, 15), true, "LIC-VS-006", "Yıllık Abonelik", "Geliştirme araçları", "Microsoft Visual Studio Professional", new DateOnly(2025, 6, 15), 30, 28, "Microsoft" },
                    { "license-007", new DateOnly(2027, 4, 1), true, "LIC-CAD-007", "Eşzamanlı Kullanım", "Teknik tasarım lisansları", "AutoCAD", new DateOnly(2026, 4, 1), 12, 9, "Autodesk" },
                    { "license-008", new DateOnly(2027, 3, 1), true, "LIC-ESET-008", "Yıllık Abonelik", "Uç nokta güvenliği", "ESET Endpoint Security", new DateOnly(2026, 3, 1), 350, 324, "ESET" },
                    { "license-009", new DateOnly(2026, 12, 1), false, "LIC-ZOOM-009", "Yıllık Abonelik", "Kullanımdan kaldırılan paket", "Zoom Workplace Business", new DateOnly(2025, 12, 1), 45, 12, "Zoom" },
                    { "license-010", new DateOnly(2026, 9, 1), false, "LIC-POWERBI-010", "Aylık Abonelik", "Geçici olarak pasife alınmış paket", "Power BI Pro", new DateOnly(2026, 8, 1), 50, 37, "Microsoft" }
                });

            migrationBuilder.CreateIndex(
                name: "UX_Licenses_LicenseCode",
                table: "Licenses",
                column: "LicenseCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Licenses");
        }
    }
}
