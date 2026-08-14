namespace TakipProgrami.Api.DTOs;

public sealed record AuditLogDto(
    string Id,
    string UserId,
    string Username,
    string EntityName,
    string EntityId,
    string Action,
    string? OldValue,
    string? NewValue,
    DateTimeOffset CreatedAt);
