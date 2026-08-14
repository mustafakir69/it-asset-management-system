using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Services;

public sealed class EmailService(
    IOptions<EmailOptions> options,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailOptions emailOptions = options.Value;

    public async Task<EmailSendResult> SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var mode = emailOptions.Mode.Trim();

        if (string.IsNullOrWhiteSpace(recipient))
        {
            const string error = "E-posta alıcısı yapılandırılmamış.";
            logger.LogWarning("Email Skipped/NotConfigured. Recipient={Recipient}, Subject={Subject}, Result={Result}, Error={Error}", recipient, subject, "Skipped", error);
            return new(EmailSendStatus.Skipped, error);
        }

        if (mode.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
        {
            const string error = "E-posta gönderimi devre dışı.";
            logger.LogInformation("Email Skipped. Recipient={Recipient}, Subject={Subject}, Body={Body}, Result={Result}, Error={Error}", recipient, subject, body, "Skipped", error);
            return new(EmailSendStatus.Skipped, error);
        }

        if (mode.Equals("LogOnly", StringComparison.OrdinalIgnoreCase))
        {
            const string error = "LogOnly modunda gerçek e-posta gönderilmedi.";
            logger.LogInformation("Email LogOnly. Recipient={Recipient}, Subject={Subject}, Body={Body}, Result={Result}, Error={Error}", recipient, subject, body, "Skipped", error);
            return new(EmailSendStatus.Skipped, error);
        }

        if (!mode.Equals("Smtp", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(emailOptions.Smtp.Host) ||
            string.IsNullOrWhiteSpace(emailOptions.FromAddress))
        {
            const string error = "SMTP veya gönderen adresi yapılandırılmamış.";
            logger.LogWarning("Email Skipped/NotConfigured. Recipient={Recipient}, Subject={Subject}, Result={Result}, Error={Error}", recipient, subject, "Skipped", error);
            return new(EmailSendStatus.Skipped, error);
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(emailOptions.FromAddress, emailOptions.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(recipient);

            using var client = new SmtpClient(emailOptions.Smtp.Host, emailOptions.Smtp.Port)
            {
                EnableSsl = emailOptions.Smtp.EnableSsl
            };
            if (!string.IsNullOrWhiteSpace(emailOptions.Smtp.Username))
            {
                client.Credentials = new NetworkCredential(
                    emailOptions.Smtp.Username,
                    emailOptions.Smtp.Password);
            }

            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("Email Sent. Recipient={Recipient}, Subject={Subject}, Result={Result}", recipient, subject, "Sent");
            return new(EmailSendStatus.Sent);
        }
        catch (Exception exception) when (exception is SmtpException or InvalidOperationException or FormatException)
        {
            logger.LogError(exception, "Email Failed. Recipient={Recipient}, Subject={Subject}, Result={Result}, Error={Error}", recipient, subject, "Failed", exception.Message);
            return new(EmailSendStatus.Failed, exception.Message);
        }
    }
}
