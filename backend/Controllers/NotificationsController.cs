using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,IT")]
[Route("api/notifications")]
public sealed class NotificationsController(NotificationService notificationService) : ControllerBase
{
    [HttpPost("process")]
    [ProducesResponseType<NotificationProcessResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationProcessResultDto>> Process(
        CancellationToken cancellationToken) =>
        Ok(await notificationService.ProcessAsync(
            DateOnly.FromDateTime(DateTime.Today),
            cancellationToken));
}
