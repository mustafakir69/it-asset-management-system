namespace TakipProgrami.Api.Helpers;

public sealed record WarrantyCalculation(int? RemainingDays, string Status);

public static class WarrantyRules
{
    public static WarrantyCalculation Calculate(DateOnly? warrantyEndDate, DateOnly today)
    {
        var remainingDays = warrantyEndDate?.DayNumber - today.DayNumber;
        var status = remainingDays switch
        {
            null => "Garanti Bilgisi Yok",
            < 0 => "Süresi Doldu",
            <= 30 => "Yaklaşıyor",
            _ => "Aktif"
        };
        return new(remainingDays, status);
    }
}
