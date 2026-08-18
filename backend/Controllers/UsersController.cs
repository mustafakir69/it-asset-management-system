using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,IT")]
[Route("api/users")]
public sealed class UsersController(UserService userService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(await userService.GetUsersAsync(cancellationToken));

    [HttpGet("username-suggestion")]
    [ProducesResponseType<UsernameSuggestionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsernameSuggestionDto>> GetUsernameSuggestion(
        [FromQuery] string employeeId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            return ProblemResponse(StatusCodes.Status400BadRequest, "Kullanıcı adı oluşturulamadı", "Çalışan seçimi zorunludur.");

        var username = await userService.SuggestUsernameAsync(employeeId.Trim(), cancellationToken);
        return username is null
            ? ProblemResponse(StatusCodes.Status400BadRequest, "Kullanıcı adı oluşturulamadı", "Ad Soyad bilgisi kullanıcı adı oluşturmak için uygun değil.")
            : Ok(new UsernameSuggestionDto(username));
    }

    [HttpGet("it-staff")]
    public async Task<ActionResult> GetItStaff([FromServices] TakipProgrami.Api.Data.ApplicationDbContext db, CancellationToken cancellationToken) =>
        Ok(await db.AppUsers.AsNoTracking().Where(x => x.IsActive && x.Role == AppRole.IT && x.EmployeeId != null)
            .OrderBy(x => x.Employee!.FullName)
            .Select(x => new { UserId = x.Id, EmployeeId = x.EmployeeId!, FullName = x.Employee!.FullName, Email = x.Email })
            .ToListAsync(cancellationToken));

    [HttpPost]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Create(
        UserCreateDto request,
        CancellationToken cancellationToken)
    {
        var roleClaim = User.FindFirstValue(ClaimTypes.Role);
        if (!Enum.TryParse<AppRole>(roleClaim, out var callerRole)) return Forbid();

        var result = await userService.CreateAsync(request, callerRole, cancellationToken);
        return result.Status switch
        {
            UserOperationStatus.Success => StatusCode(StatusCodes.Status201Created, result.User),
            UserOperationStatus.Forbidden => ProblemResponse(
                StatusCodes.Status403Forbidden,
                "Yetkisiz işlem",
                result.ErrorMessage!),
            UserOperationStatus.Conflict => ProblemResponse(
                StatusCodes.Status409Conflict,
                "Kullanıcı oluşturulamadı",
                result.ErrorMessage!),
            _ => ProblemResponse(
                StatusCodes.Status400BadRequest,
                "Kullanıcı oluşturulamadı",
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
}
