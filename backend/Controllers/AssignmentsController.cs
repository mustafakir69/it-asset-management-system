using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Helpers;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/assignments")]
public sealed class AssignmentsController(AssignmentService assignmentService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,IT")]
    [ProducesResponseType<IReadOnlyList<AssignmentDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssignmentDto>>> GetActive(
        string? search,
        string? department,
        CancellationToken cancellationToken) =>
        Ok(await assignmentService.GetActiveAsync(search, department, cancellationToken));

    [HttpGet("history")]
    [Authorize(Roles = "Admin,IT")]
    [ProducesResponseType<IReadOnlyList<AssignmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AssignmentDto>>> GetHistory(
        string? search,
        string? department,
        string? status,
        CancellationToken cancellationToken)
    {
        if (!TryParseStatus(status, out var isActive))
        {
            return ProblemResponse(
                StatusCodes.Status400BadRequest,
                "Geçersiz durum",
                "Durum filtresi Aktif veya İade Edildi olmalıdır.");
        }

        return Ok(await assignmentService.GetHistoryAsync(
            search,
            department,
            isActive,
            cancellationToken));
    }

    [HttpGet("my")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType<IReadOnlyList<AssignmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AssignmentDto>>> GetMy(
        CancellationToken cancellationToken)
    {
        var employeeId = User.FindFirstValue("employeeId");
        if (string.IsNullOrWhiteSpace(employeeId)) return Forbid();

        return Ok(await assignmentService.GetMyActiveAsync(employeeId, cancellationToken));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var assignment = await assignmentService.GetByIdAsync(id, cancellationToken);
        if (assignment is null) return NotFound();
        if (User.IsInRole("Employee") && assignment.EmployeeId != User.GetEmployeeId()) return Forbid();
        return Ok(assignment);
    }

    [HttpPost]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssignmentDto>> Create(
        AssignmentCreateDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        var result = await assignmentService.CreateAsync(request, userId, cancellationToken);
        return result.Status switch
        {
            AssignmentOperationStatus.Success => CreatedAtAction(
                nameof(GetById),
                new { id = result.Assignment!.Id },
                result.Assignment),
            AssignmentOperationStatus.Conflict => ProblemResponse(
                StatusCodes.Status409Conflict,
                "Zimmet oluşturulamadı",
                result.ErrorMessage!),
            _ => ProblemResponse(
                StatusCodes.Status400BadRequest,
                "Zimmet oluşturulamadı",
                result.ErrorMessage!)
        };
    }

    [HttpPut("{id}/return")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssignmentDto>> Return(
        string id,
        AssignmentReturnDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        var result = await assignmentService.ReturnAsync(id, request, userId, cancellationToken);
        return result.Status switch
        {
            AssignmentOperationStatus.Success => Ok(result.Assignment),
            AssignmentOperationStatus.NotFound => ProblemResponse(
                StatusCodes.Status404NotFound,
                "Zimmet bulunamadı",
                result.ErrorMessage!),
            AssignmentOperationStatus.Conflict => ProblemResponse(
                StatusCodes.Status409Conflict,
                "İade alınamadı",
                result.ErrorMessage!),
            _ => ProblemResponse(
                StatusCodes.Status400BadRequest,
                "İade alınamadı",
                result.ErrorMessage!)
        };
    }

    private ObjectResult ProblemResponse(int statusCode, string title, string detail) =>
        StatusCode(statusCode, new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        });

    private static bool TryParseStatus(string? status, out bool? isActive)
    {
        isActive = null;
        if (string.IsNullOrWhiteSpace(status)) return true;
        if (status.Trim().Equals("Aktif", StringComparison.CurrentCultureIgnoreCase))
        {
            isActive = true;
            return true;
        }
        if (status.Trim().Equals("İade Edildi", StringComparison.CurrentCultureIgnoreCase))
        {
            isActive = false;
            return true;
        }
        return false;
    }
}
