using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialAssetSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssetCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WarrantyEndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "AssetCode", "Brand", "Category", "Location", "Model", "PurchaseDate", "SerialNumber", "Status", "WarrantyEndDate" },
                values: new object[,]
                {
                    { "asset-db-001", "DNT-2026-1001", "Lenovo", "Dizüstü Bilgisayar", "İstanbul Merkez", "ThinkPad T14 Gen 5", new DateOnly(2026, 2, 12), "DB-LNV-T14-1001", "Zimmetli", new DateOnly(2029, 2, 12) },
                    { "asset-db-002", "DNT-2026-1002", "Dell", "Dizüstü Bilgisayar", "Ankara Ofis", "Latitude 5450", new DateOnly(2026, 1, 28), "DB-DLL-L54-1002", "Stokta", new DateOnly(2029, 1, 28) },
                    { "asset-db-003", "DNT-2025-1003", "HP", "Dizüstü Bilgisayar", "İstanbul Merkez", "EliteBook 840 G11", new DateOnly(2025, 10, 6), "DB-HP-840-1003", "Bakımda", new DateOnly(2028, 10, 6) },
                    { "asset-db-004", "MST-2026-1004", "Dell", "Masaüstü Bilgisayar", "İstanbul Depo", "OptiPlex 7020", new DateOnly(2026, 3, 14), "DB-DLL-OP7-1004", "Stokta", new DateOnly(2029, 3, 14) },
                    { "asset-db-005", "MST-2024-1005", "HP", "Masaüstü Bilgisayar", "İstanbul Depo", "Pro Tower 400 G9", new DateOnly(2021, 5, 17), "DB-HP-400-1005", "Hurda", new DateOnly(2024, 5, 17) },
                    { "asset-db-006", "MNT-2026-1006", "Dell", "Monitör", "İstanbul Depo", "P2725H 27 inç", new DateOnly(2026, 4, 8), "DB-DLL-P27-1006", "Stokta", new DateOnly(2029, 4, 8) },
                    { "asset-db-007", "MNT-2025-1007", "Samsung", "Monitör", "İzmir Ofis", "ViewFinity S6 27 inç", new DateOnly(2025, 12, 15), "DB-SMS-S6-1007", "Kayıp", new DateOnly(2028, 12, 15) },
                    { "asset-db-008", "TLF-2026-1008", "Apple", "Telefon", "İstanbul Merkez", "iPhone 16", new DateOnly(2026, 2, 2), "DB-APL-IP16-1008", "Zimmetli", new DateOnly(2028, 2, 2) },
                    { "asset-db-009", "TLF-2023-1009", "Samsung", "Telefon", "İstanbul Depo", "Galaxy S22", new DateOnly(2022, 3, 18), "DB-SMS-S22-1009", "Elden çıkarıldı", new DateOnly(2024, 3, 18) },
                    { "asset-db-010", "YAZ-2026-1010", "Canon", "Yazıcı", "İstanbul Depo", "i-SENSYS MF655Cdw", new DateOnly(2026, 5, 6), "DB-CNN-MF6-1010", "Stokta", new DateOnly(2028, 5, 6) }
                });

            migrationBuilder.CreateIndex(
                name: "UX_Assets_AssetCode",
                table: "Assets",
                column: "AssetCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Assets_SerialNumber",
                table: "Assets",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");
        }
    }
}
