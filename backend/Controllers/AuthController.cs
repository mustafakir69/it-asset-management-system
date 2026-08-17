using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/auth")]
public sealed class AuthController(
    ApplicationDbContext dbContext,
    IPasswordHasher<AppUser> passwordHasher,
    JwtTokenService jwtTokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var identifier = request.Identifier.Trim();
        var user = await dbContext.AppUsers
            .Include(current => current.Employee)
            .FirstOrDefaultAsync(
                current => current.Username == identifier || current.Email == identifier,
                cancellationToken);

        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Giriş başarısız",
                Detail = "Kullanıcı adı/e-posta veya parola hatalı."
            });
        }

        if (!user.IsActive)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Giriş yapılamadı",
                Detail = "Kullanıcı hesabı pasif durumda."
            });
        }

        if (!Enum.IsDefined(user.Role))
        {
            return Unauthorized(new ProblemDetails { Status = 401, Title = "Giriş başarısız", Detail = "Kullanıcı rolü artık desteklenmiyor." });
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var token = jwtTokenService.CreateToken(user);
        return Ok(new LoginResponseDto(token.Token, token.ExpiresAt, ToDto(user)));
    }

    [HttpGet("me")]
    [ProducesResponseType<AuthUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthUserDto>> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var user = await dbContext.AppUsers
            .AsNoTracking()
            .Include(current => current.Employee)
            .FirstOrDefaultAsync(current => current.Id == userId, cancellationToken);

        if (user is null) return Unauthorized();
        if (!user.IsActive) return Forbid();

        return Ok(ToDto(user));
    }

    private static AuthUserDto ToDto(AppUser user) => new(
        user.Id,
        user.EmployeeId,
        DisplayName(user.Employee?.FullName, user.Username),
        Clean(user.Employee?.Department),
        user.Username,
        user.Email,
        user.Role.ToString(),
        GetRoleDisplayName(user.Role));

    private static string GetRoleDisplayName(AppRole role) => role switch
    {
        AppRole.Admin => "Yönetici",
        AppRole.IT => "IT Yetkilisi",
        AppRole.Employee => "Çalışan",
        _ => role.ToString()
    };

    private static string DisplayName(string? fullName, string username) =>
        string.IsNullOrWhiteSpace(fullName) ? username : fullName.Trim();

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
