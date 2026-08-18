using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Helpers;

public static class AssetLifecycleRules
{
    public const string Available = "Boşta";
    public const string Assigned = "Zimmetli";
    public const string InMaintenance = "Bakımda";
    public const string Lost = "Kayıp";
    public const string Scrapped = "Hurda";
    public const string Disposed = "Elden Çıkarıldı";

    public static readonly IReadOnlySet<string> ValidStatuses = new HashSet<string>(
        [Available, Assigned, InMaintenance, Lost, Scrapped, Disposed],
        StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlySet<string> ScrapReasons = new HashSet<string>(
        ["Ekonomik onarım mümkün değil", "Fiziksel hasar", "Donanım ömrünü tamamladı", "Diğer"],
        StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlySet<string> DisposalMethods = new HashSet<string>(
        ["Satış", "Bağış", "İmha", "Diğer"],
        StringComparer.OrdinalIgnoreCase);

    public static bool IsCritical(string status) =>
        status.Equals(Lost, StringComparison.OrdinalIgnoreCase) ||
        status.Equals(Scrapped, StringComparison.OrdinalIgnoreCase) ||
        status.Equals(Disposed, StringComparison.OrdinalIgnoreCase);

    public static string MovementDisplayName(AssetMovementType movementType) => movementType switch
    {
        AssetMovementType.InventoryCreated => "Envantere Eklendi",
        AssetMovementType.InformationUpdated => "Bilgileri Güncellendi",
        AssetMovementType.Assigned => "Zimmetlendi",
        AssetMovementType.Returned => "İade Alındı",
        AssetMovementType.MaintenanceStarted => "Bakıma Alındı",
        AssetMovementType.MaintenanceCompleted => "Bakım Tamamlandı",
        AssetMovementType.StatusChanged => "Durum Değiştirildi",
        AssetMovementType.MarkedLost => "Kayıp Olarak İşaretlendi",
        AssetMovementType.Scrapped => "Hurdaya Ayrıldı",
        AssetMovementType.Disposed => "Elden Çıkarıldı",
        _ => movementType.ToString()
    };
}
