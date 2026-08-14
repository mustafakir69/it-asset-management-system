using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Auditor")]
[Route("api/audit-logs")]
public sealed class AuditLogsController(AuditLogService auditLogService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AuditLogDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetAll(
        string? entityName,
        string? action,
        string? username,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken) =>
        Ok(await auditLogService.GetAsync(entityName, action, username, from, to, cancellationToken));
}
