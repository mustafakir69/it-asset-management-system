using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.Helpers;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Tests;

internal static class TestInfrastructure
{
    public static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new(options, new HttpContextAccessor());
    }

    public static NotificationService CreateNotificationService(
        ApplicationDbContext dbContext,
        IEmailService? emailService = null) =>
        new(
            dbContext,
            emailService ?? new FakeEmailService(),
            Options.Create(new EmailOptions
            {
                Mode = "LogOnly",
                StockRecipient = "stock@example.test",
                MaintenanceRecipient = "maintenance@example.test"
            }),
            NullLogger<NotificationService>.Instance);
}

internal sealed class FakeEmailService(
    EmailSendStatus status = EmailSendStatus.Skipped) : IEmailService
{
    public int CallCount { get; private set; }

    public Task<EmailSendResult> SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(new EmailSendResult(status));
    }
}
