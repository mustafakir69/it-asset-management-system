using System.Security.Claims;
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
    [HttpGet("summary"), Authorize(Roles = "Admin,IT")]
    [ProducesResponseType<DashboardSummaryDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
        CancellationToken cancellationToken) =>
        Ok(await dashboardService.GetSummaryAsync(cancellationToken));

    [HttpGet("my-summary"), Authorize(Roles = "Employee")]
    public async Task<ActionResult<EmployeeDashboardDto>> GetMySummary(CancellationToken cancellationToken)
    {
        var employeeId = User.FindFirstValue("employeeId");
        return string.IsNullOrWhiteSpace(employeeId) ? Forbid() : Ok(await dashboardService.GetMySummaryAsync(employeeId, cancellationToken));
    }
}
