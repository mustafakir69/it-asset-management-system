using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();

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
    }
}
