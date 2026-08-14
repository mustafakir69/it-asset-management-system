using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TakipProgrami.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BrandModel = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: false),
                    MinimumQuantity = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockItems", x => x.Id);
                    table.CheckConstraint("CK_StockItems_CurrentQuantity_NonNegative", "[CurrentQuantity] >= 0");
                    table.CheckConstraint("CK_StockItems_MinimumQuantity_NonNegative", "[MinimumQuantity] >= 0");
                });

            migrationBuilder.InsertData(
                table: "StockItems",
                columns: new[] { "Id", "BrandModel", "Category", "CurrentQuantity", "IsActive", "ItemCode", "Location", "MinimumQuantity", "Name", "Unit" },
                values: new object[,]
                {
                    { "stock-item-001", "Logitech M185", "Çevre Birimi", 35, true, "STK-2026-0001", "İstanbul Depo", 10, "Kablosuz Mouse", "Adet" },
                    { "stock-item-002", "Logitech K120", "Çevre Birimi", 18, true, "STK-2026-0002", "İstanbul Depo", 8, "Kablolu Klavye", "Adet" },
                    { "stock-item-003", "Jabra Evolve2 30", "Çevre Birimi", 6, true, "STK-2026-0003", "Ankara Ofis", 5, "USB Kulaklık", "Adet" },
                    { "stock-item-004", "Ugreen High Speed", "Kablo", 12, true, "STK-2026-0004", "İstanbul Depo", 12, "HDMI Kablo 2 m", "Adet" },
                    { "stock-item-005", "Ugreen DP 1.4", "Kablo", 4, true, "STK-2026-0005", "İstanbul Depo", 8, "DisplayPort Kablo 1,8 m", "Adet" },
                    { "stock-item-006", "Digitus Cat6", "Kablo", 50, true, "STK-2026-0006", "İstanbul Depo", 20, "Ethernet Kablo 3 m", "Adet" },
                    { "stock-item-007", "Kingston DataTraveler Exodia", "Depolama", 7, true, "STK-2026-0007", "Ankara Ofis", 10, "USB Bellek 64 GB", "Adet" },
                    { "stock-item-008", "HP 59A", "Sarf Malzeme", 3, true, "STK-2026-0008", "İstanbul Depo", 5, "Siyah Toner", "Adet" },
                    { "stock-item-009", "Baseus Metal Gleam", "Adaptör", 14, true, "STK-2026-0009", "İzmir Ofis", 6, "USB-C HDMI Adaptör", "Adet" },
                    { "stock-item-010", "Lenovo USB-C 65W", "Güç Aksesuarı", 5, true, "STK-2026-0010", "İstanbul Depo", 5, "Laptop Şarj Adaptörü", "Adet" },
                    { "stock-item-011", "Dell USB-C 65W", "Güç Aksesuarı", 9, true, "STK-2026-0011", "Ankara Ofis", 4, "Laptop Şarj Adaptörü", "Adet" },
                    { "stock-item-012", "Duracell Alkalin 10'lu", "Sarf Malzeme", 40, true, "STK-2026-0012", "İstanbul Depo", 20, "AA Pil", "Paket" },
                    { "stock-item-013", "Samsung 990 EVO Plus", "Depolama", 6, true, "STK-2026-0013", "İstanbul Depo", 3, "SSD 1 TB", "Adet" },
                    { "stock-item-014", "Logitech C920", "Çevre Birimi", 3, true, "STK-2026-0014", "Bursa Şube", 4, "Web Kamera", "Adet" },
                    { "stock-item-015", "Ugreen 100W", "Kablo", 22, true, "STK-2026-0015", "İstanbul Depo", 10, "USB-C Kablo 2 m", "Adet" },
                    { "stock-item-016", "Schneider 6'lı", "Güç Aksesuarı", 8, true, "STK-2026-0016", "İzmir Ofis", 5, "Akım Korumalı Priz", "Adet" },
                    { "stock-item-017", "Arctic MX-6 4 g", "Sarf Malzeme", 2, true, "STK-2026-0017", "İstanbul Depo", 3, "Termal Macun", "Adet" },
                    { "stock-item-018", "Brother TZe-231", "Sarf Malzeme", 15, true, "STK-2026-0018", "Ankara Ofis", 6, "Etiket Şeridi", "Adet" }
                });

            migrationBuilder.CreateIndex(
                name: "UX_StockItems_ItemCode",
                table: "StockItems",
                column: "ItemCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockItems");
        }
    }
}
