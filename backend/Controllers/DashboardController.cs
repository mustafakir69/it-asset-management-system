using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(DashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType<DashboardSummaryDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
        CancellationToken cancellationToken) =>
        Ok(await dashboardService.GetSummaryAsync(cancellationToken));
}
