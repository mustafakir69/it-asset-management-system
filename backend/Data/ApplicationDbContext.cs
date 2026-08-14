using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

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

        asset.HasData(
            new Asset
            {
                Id = "asset-db-001",
                AssetCode = "DNT-2026-1001",
                Category = "Dizüstü Bilgisayar",
                Brand = "Lenovo",
                Model = "ThinkPad T14 Gen 5",
                SerialNumber = "DB-LNV-T14-1001",
                Status = "Zimmetli",
                Location = "İstanbul Merkez",
                PurchaseDate = new DateOnly(2026, 2, 12),
                WarrantyEndDate = new DateOnly(2029, 2, 12)
            },
            new Asset
            {
                Id = "asset-db-002",
                AssetCode = "DNT-2026-1002",
                Category = "Dizüstü Bilgisayar",
                Brand = "Dell",
                Model = "Latitude 5450",
                SerialNumber = "DB-DLL-L54-1002",
                Status = "Stokta",
                Location = "Ankara Ofis",
                PurchaseDate = new DateOnly(2026, 1, 28),
                WarrantyEndDate = new DateOnly(2029, 1, 28)
            },
            new Asset
            {
                Id = "asset-db-003",
                AssetCode = "DNT-2025-1003",
                Category = "Dizüstü Bilgisayar",
                Brand = "HP",
                Model = "EliteBook 840 G11",
                SerialNumber = "DB-HP-840-1003",
                Status = "Bakımda",
                Location = "İstanbul Merkez",
                PurchaseDate = new DateOnly(2025, 10, 6),
                WarrantyEndDate = new DateOnly(2028, 10, 6)
            },
            new Asset
            {
                Id = "asset-db-004",
                AssetCode = "MST-2026-1004",
                Category = "Masaüstü Bilgisayar",
                Brand = "Dell",
                Model = "OptiPlex 7020",
                SerialNumber = "DB-DLL-OP7-1004",
                Status = "Stokta",
                Location = "İstanbul Depo",
                PurchaseDate = new DateOnly(2026, 3, 14),
                WarrantyEndDate = new DateOnly(2029, 3, 14)
            },
            new Asset
            {
                Id = "asset-db-005",
                AssetCode = "MST-2024-1005",
                Category = "Masaüstü Bilgisayar",
                Brand = "HP",
                Model = "Pro Tower 400 G9",
                SerialNumber = "DB-HP-400-1005",
                Status = "Hurda",
                Location = "İstanbul Depo",
                PurchaseDate = new DateOnly(2021, 5, 17),
                WarrantyEndDate = new DateOnly(2024, 5, 17)
            },
            new Asset
            {
                Id = "asset-db-006",
                AssetCode = "MNT-2026-1006",
                Category = "Monitör",
                Brand = "Dell",
                Model = "P2725H 27 inç",
                SerialNumber = "DB-DLL-P27-1006",
                Status = "Stokta",
                Location = "İstanbul Depo",
                PurchaseDate = new DateOnly(2026, 4, 8),
                WarrantyEndDate = new DateOnly(2029, 4, 8)
            },
            new Asset
            {
                Id = "asset-db-007",
                AssetCode = "MNT-2025-1007",
                Category = "Monitör",
                Brand = "Samsung",
                Model = "ViewFinity S6 27 inç",
                SerialNumber = "DB-SMS-S6-1007",
                Status = "Kayıp",
                Location = "İzmir Ofis",
                PurchaseDate = new DateOnly(2025, 12, 15),
                WarrantyEndDate = new DateOnly(2028, 12, 15)
            },
            new Asset
            {
                Id = "asset-db-008",
                AssetCode = "TLF-2026-1008",
                Category = "Telefon",
                Brand = "Apple",
                Model = "iPhone 16",
                SerialNumber = "DB-APL-IP16-1008",
                Status = "Zimmetli",
                Location = "İstanbul Merkez",
                PurchaseDate = new DateOnly(2026, 2, 2),
                WarrantyEndDate = new DateOnly(2028, 2, 2)
            },
            new Asset
            {
                Id = "asset-db-009",
                AssetCode = "TLF-2023-1009",
                Category = "Telefon",
                Brand = "Samsung",
                Model = "Galaxy S22",
                SerialNumber = "DB-SMS-S22-1009",
                Status = "Elden çıkarıldı",
                Location = "İstanbul Depo",
                PurchaseDate = new DateOnly(2022, 3, 18),
                WarrantyEndDate = new DateOnly(2024, 3, 18)
            },
            new Asset
            {
                Id = "asset-db-010",
                AssetCode = "YAZ-2026-1010",
                Category = "Yazıcı",
                Brand = "Canon",
                Model = "i-SENSYS MF655Cdw",
                SerialNumber = "DB-CNN-MF6-1010",
                Status = "Stokta",
                Location = "İstanbul Depo",
                PurchaseDate = new DateOnly(2026, 5, 6),
                WarrantyEndDate = new DateOnly(2028, 5, 6)
            });

        ConfigureStockItems(modelBuilder);
        ConfigureStockTransactions(modelBuilder);
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

        stockItem.HasData(
            new StockItem
            {
                Id = "stock-item-001",
                ItemCode = "STK-2026-0001",
                Name = "Kablosuz Mouse",
                Category = "Çevre Birimi",
                BrandModel = "Logitech M185",
                Unit = "Adet",
                CurrentQuantity = 35,
                MinimumQuantity = 10,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-002",
                ItemCode = "STK-2026-0002",
                Name = "Kablolu Klavye",
                Category = "Çevre Birimi",
                BrandModel = "Logitech K120",
                Unit = "Adet",
                CurrentQuantity = 18,
                MinimumQuantity = 8,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-003",
                ItemCode = "STK-2026-0003",
                Name = "USB Kulaklık",
                Category = "Çevre Birimi",
                BrandModel = "Jabra Evolve2 30",
                Unit = "Adet",
                CurrentQuantity = 6,
                MinimumQuantity = 5,
                Location = "Ankara Ofis",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-004",
                ItemCode = "STK-2026-0004",
                Name = "HDMI Kablo 2 m",
                Category = "Kablo",
                BrandModel = "Ugreen High Speed",
                Unit = "Adet",
                CurrentQuantity = 12,
                MinimumQuantity = 12,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-005",
                ItemCode = "STK-2026-0005",
                Name = "DisplayPort Kablo 1,8 m",
                Category = "Kablo",
                BrandModel = "Ugreen DP 1.4",
                Unit = "Adet",
                CurrentQuantity = 4,
                MinimumQuantity = 8,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-006",
                ItemCode = "STK-2026-0006",
                Name = "Ethernet Kablo 3 m",
                Category = "Kablo",
                BrandModel = "Digitus Cat6",
                Unit = "Adet",
                CurrentQuantity = 50,
                MinimumQuantity = 20,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-007",
                ItemCode = "STK-2026-0007",
                Name = "USB Bellek 64 GB",
                Category = "Depolama",
                BrandModel = "Kingston DataTraveler Exodia",
                Unit = "Adet",
                CurrentQuantity = 7,
                MinimumQuantity = 10,
                Location = "Ankara Ofis",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-008",
                ItemCode = "STK-2026-0008",
                Name = "Siyah Toner",
                Category = "Sarf Malzeme",
                BrandModel = "HP 59A",
                Unit = "Adet",
                CurrentQuantity = 3,
                MinimumQuantity = 5,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-009",
                ItemCode = "STK-2026-0009",
                Name = "USB-C HDMI Adaptör",
                Category = "Adaptör",
                BrandModel = "Baseus Metal Gleam",
                Unit = "Adet",
                CurrentQuantity = 14,
                MinimumQuantity = 6,
                Location = "İzmir Ofis",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-010",
                ItemCode = "STK-2026-0010",
                Name = "Laptop Şarj Adaptörü",
                Category = "Güç Aksesuarı",
                BrandModel = "Lenovo USB-C 65W",
                Unit = "Adet",
                CurrentQuantity = 5,
                MinimumQuantity = 5,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-011",
                ItemCode = "STK-2026-0011",
                Name = "Laptop Şarj Adaptörü",
                Category = "Güç Aksesuarı",
                BrandModel = "Dell USB-C 65W",
                Unit = "Adet",
                CurrentQuantity = 9,
                MinimumQuantity = 4,
                Location = "Ankara Ofis",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-012",
                ItemCode = "STK-2026-0012",
                Name = "AA Pil",
                Category = "Sarf Malzeme",
                BrandModel = "Duracell Alkalin 10'lu",
                Unit = "Paket",
                CurrentQuantity = 40,
                MinimumQuantity = 20,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-013",
                ItemCode = "STK-2026-0013",
                Name = "SSD 1 TB",
                Category = "Depolama",
                BrandModel = "Samsung 990 EVO Plus",
                Unit = "Adet",
                CurrentQuantity = 6,
                MinimumQuantity = 3,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-014",
                ItemCode = "STK-2026-0014",
                Name = "Web Kamera",
                Category = "Çevre Birimi",
                BrandModel = "Logitech C920",
                Unit = "Adet",
                CurrentQuantity = 3,
                MinimumQuantity = 4,
                Location = "Bursa Şube",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-015",
                ItemCode = "STK-2026-0015",
                Name = "USB-C Kablo 2 m",
                Category = "Kablo",
                BrandModel = "Ugreen 100W",
                Unit = "Adet",
                CurrentQuantity = 22,
                MinimumQuantity = 10,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-016",
                ItemCode = "STK-2026-0016",
                Name = "Akım Korumalı Priz",
                Category = "Güç Aksesuarı",
                BrandModel = "Schneider 6'lı",
                Unit = "Adet",
                CurrentQuantity = 8,
                MinimumQuantity = 5,
                Location = "İzmir Ofis",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-017",
                ItemCode = "STK-2026-0017",
                Name = "Termal Macun",
                Category = "Sarf Malzeme",
                BrandModel = "Arctic MX-6 4 g",
                Unit = "Adet",
                CurrentQuantity = 2,
                MinimumQuantity = 3,
                Location = "İstanbul Depo",
                IsActive = true
            },
            new StockItem
            {
                Id = "stock-item-018",
                ItemCode = "STK-2026-0018",
                Name = "Etiket Şeridi",
                Category = "Sarf Malzeme",
                BrandModel = "Brother TZe-231",
                Unit = "Adet",
                CurrentQuantity = 15,
                MinimumQuantity = 6,
                Location = "Ankara Ofis",
                IsActive = true
            });
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
        stockTransaction.Property(transaction => transaction.PersonName)
            .HasMaxLength(150)
            .IsRequired();
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
    }
}
