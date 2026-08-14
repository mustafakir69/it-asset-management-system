namespace TakipProgrami.Api.Services;

public enum EmailSendStatus
{
    Sent,
    Failed,
    Skipped
}

public sealed record EmailSendResult(EmailSendStatus Status, string? ErrorMessage = null);

public interface IEmailService
{
    Task<EmailSendResult> SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken);
}
