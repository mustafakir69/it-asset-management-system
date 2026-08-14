using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;

namespace TakipProgrami.Api.Services;

public sealed class AuditLogService(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<AuditLogDto>> GetAsync(
        string? entityName,
        string? action,
        string? username,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(entityName)) query = query.Where(item => item.EntityName == entityName.Trim());
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(item => item.Action == action.Trim());
        if (!string.IsNullOrWhiteSpace(username)) query = query.Where(item => item.Username.Contains(username.Trim()));
        if (from.HasValue) query = query.Where(item => item.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(item => item.CreatedAt <= to.Value);

        return await query.OrderByDescending(item => item.CreatedAt)
            .Select(item => new AuditLogDto(
                item.Id, item.UserId, item.Username, item.EntityName, item.EntityId,
                item.Action, item.OldValue, item.NewValue, item.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
