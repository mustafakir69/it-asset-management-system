using System.Globalization;
using System.Text;
using TakipProgrami.Api.DTOs;

namespace TakipProgrami.Api.Helpers;

public static class CsvExporter
{
    public static byte[] Inventory(IEnumerable<InventoryReportDto> records) => Create(
        ["Varlık Kodu", "Kategori", "Marka", "Model", "Seri Numarası", "Durum", "Lokasyon", "Satın Alma Tarihi", "Garanti Bitiş Tarihi"],
        records.Select(item => new[] { item.AssetCode, item.Category, item.Brand, item.Model, item.SerialNumber, item.Status, item.Location, Date(item.PurchaseDate), Date(item.WarrantyEndDate) }));

    public static byte[] Assignments(IEnumerable<AssignmentReportDto> records) => Create(
        ["Varlık Kodu", "Cihaz", "Çalışan", "Departman", "Zimmet Tarihi", "İade Tarihi", "Durum", "Zimmetleyen", "İade Alan"],
        records.Select(item => new[] { item.AssetCode, item.AssetName, item.EmployeeName, item.Department, DateTime(item.AssignedAt), item.ReturnedAt.HasValue ? DateTime(item.ReturnedAt.Value) : "", item.Status, item.AssignedBy, item.ReturnedBy ?? "" }));

    public static byte[] Stock(IEnumerable<StockReportDto> records) => Create(
        ["Ürün Kodu", "Ürün", "Kategori", "Marka / Model", "Birim", "Mevcut Stok", "Minimum Stok", "Kritik", "Lokasyon"],
        records.Select(item => new[] { item.ItemCode, item.ItemName, item.Category, item.BrandModel, item.Unit, item.CurrentQuantity.ToString(CultureInfo.InvariantCulture), item.MinimumQuantity.ToString(CultureInfo.InvariantCulture), item.IsCritical ? "Kritik" : "Normal", item.Location }));

    public static byte[] Maintenance(IEnumerable<MaintenanceReportDto> records) => Create(
        ["Cihaz Kodu", "Cihaz", "Bakım / Talep", "Kayıt Türü", "Planlanan Tarih", "Gerçekleşen Tarih", "Yapan Kişi", "Sonuç", "Durum"],
        records.Select(item => new[] { item.AssetCode, item.AssetName, item.Title, item.RecordType, item.PlannedDate.HasValue ? Date(item.PlannedDate.Value) : "", item.CompletedAt.HasValue ? DateTime(item.CompletedAt.Value) : "", item.PerformedBy ?? "", item.Result ?? "", item.Status }));

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
