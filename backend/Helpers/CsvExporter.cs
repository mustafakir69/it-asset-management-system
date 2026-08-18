using System.Globalization;
using System.Text;
using TakipProgrami.Api.DTOs;

namespace TakipProgrami.Api.Helpers;

public static class CsvExporter
{
    public static byte[] Inventory(IEnumerable<InventoryReportDto> records) => Create(
        ["Varlık Kodu", "Kategori", "Marka", "Model", "Seri Numarası", "Durum", "Lokasyon", "Satın Alma Tarihi", "Garanti Bitiş Tarihi"],
        records.Select(item => new[] { item.AssetCode, item.Category, item.Brand, item.Model, item.SerialNumber, item.Status, item.Location, Date(item.PurchaseDate), item.WarrantyEndDate.HasValue ? Date(item.WarrantyEndDate.Value) : "" }));

    public static byte[] Assignments(IEnumerable<AssignmentReportDto> records) => Create(
        ["Varlık Kodu", "Cihaz", "Çalışan", "Departman", "Zimmet Tarihi", "İade Tarihi", "Durum", "Zimmetleyen", "İade Alan"],
        records.Select(item => new[] { item.AssetCode, item.AssetName, item.EmployeeName, item.Department, DateTime(item.AssignedAt), item.ReturnedAt.HasValue ? DateTime(item.ReturnedAt.Value) : "", item.Status, item.AssignedByName, item.ReturnedByName ?? "" }));

    public static byte[] Stock(IEnumerable<StockReportDto> records) => Create(
        ["Ürün Kodu", "Ürün", "Kategori", "Marka / Model", "Birim", "Mevcut Stok", "Minimum Stok", "Kritik", "Lokasyon"],
        records.Select(item => new[] { item.ItemCode, item.ItemName, item.Category, item.BrandModel, item.Unit, item.CurrentQuantity.ToString(CultureInfo.InvariantCulture), item.MinimumQuantity.ToString(CultureInfo.InvariantCulture), item.IsCritical ? "Kritik" : "Normal", item.Location }));

    public static byte[] Maintenance(IEnumerable<MaintenanceReportDto> records) => Create(
        ["Cihaz Kodu", "Cihaz", "Bakım / Talep", "Kayıt Türü", "Planlanan Tarih", "Gerçekleşen Tarih", "Yapan Kişi", "Sonuç", "Durum"],
        records.Select(item => new[] { item.AssetCode, item.AssetName, item.Title, item.RecordType, item.PlannedDate.HasValue ? Date(item.PlannedDate.Value) : "", item.CompletedAt.HasValue ? DateTime(item.CompletedAt.Value) : "", item.ActorName ?? "", item.Result ?? "", item.Status }));

    public static byte[] Warranties(IEnumerable<WarrantyReportDto> records) => Create(
        ["Cihaz Kodu", "Cihaz", "Kategori", "Garanti Bitişi", "Garanti Durumu", "Kullanım Durumu", "Zimmetli Çalışan", "Birim"],
        records.Select(item => new[] { item.AssetCode, item.AssetName, item.Category, item.WarrantyEndDate.HasValue ? Date(item.WarrantyEndDate.Value) : "", item.WarrantyStatus, item.AssetStatus, item.CurrentAssigneeName ?? "", item.CurrentAssigneeDepartment ?? "" }));

    public static byte[] Licenses(IEnumerable<LicenseReportDto> records) => Create(
        ["Lisans Kodu", "Ürün", "Sağlayıcı", "Lisans Türü", "Toplam Hak", "Kullanılan", "Kalan", "Bitiş Tarihi", "Durum"],
        records.Select(item => new[] { item.LicenseCode, item.ProductName, item.Vendor, item.LicenseType, item.TotalSeats.ToString(CultureInfo.InvariantCulture), item.UsedSeats.ToString(CultureInfo.InvariantCulture), item.AvailableSeats.ToString(CultureInfo.InvariantCulture), item.ExpirationDate.HasValue ? Date(item.ExpirationDate.Value) : "", item.Status }));

    public static byte[] SupportRequests(IEnumerable<SupportReportDto> records) => Create(
        ["Talep No", "Cihaz Kodu", "Cihaz", "Talebi Açan", "Birim", "Öncelik", "Durum", "Atanan IT", "Oluşturulma", "Tamamlanma", "Tamamlayan", "Çözüm"],
        records.Select(item => new[] { item.RequestNumber, item.AssetCode, item.AssetName, item.RequestedByName, item.Department, item.Priority, item.Status, item.AssignedToName ?? "", DateTime(item.CreatedAt), item.CompletedAt.HasValue ? DateTime(item.CompletedAt.Value) : "", item.CompletedByName ?? "", item.Result ?? "" }));

    private static byte[] Create(IEnumerable<string> headers, IEnumerable<string[]> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(';', headers.Select(Escape)));
        foreach (var row in rows) builder.AppendLine(string.Join(';', row.Select(Escape)));
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
    }

    private static string Escape(string value) =>
        $"\"{value.Replace("\"", "\"\"")}\"";

    private static string Date(DateOnly value) => value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
    private static string DateTime(DateTimeOffset value) => value.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
}
