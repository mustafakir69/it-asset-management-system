namespace TakipProgrami.Api.Helpers;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Mode { get; set; } = "Disabled";
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Takip Programı";
    public string StockRecipient { get; set; } = string.Empty;
    public string MaintenanceRecipient { get; set; } = string.Empty;
    public SmtpOptions Smtp { get; set; } = new();
}

public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
