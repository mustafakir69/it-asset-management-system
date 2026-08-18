using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Helpers;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/search")]
public sealed class GlobalSearchController(GlobalSearchService searchService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GlobalSearchResultDto>>> Search(
        string query,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var term = query?.Trim() ?? string.Empty;
        if (term.Length < 2)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Arama ifadesi çok kısa",
                Detail = "Arama için en az 2 karakter girin."
            });
        if (term.Length > 100)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Arama ifadesi çok uzun",
                Detail = "Arama ifadesi en fazla 100 karakter olabilir."
            });

        var isEmployee = User.IsInRole("Employee");
        var employeeId = isEmployee ? User.GetEmployeeId() : null;
        if (isEmployee && employeeId is null) return Forbid();
        return Ok(await searchService.SearchAsync(
            term,
            Math.Clamp(limit, 1, 8),
            isEmployee,
            employeeId,
            cancellationToken));
    }
}
