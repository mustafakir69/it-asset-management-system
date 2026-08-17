using System.Security.Claims;

namespace TakipProgrami.Api.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier);

    public static string? GetEmployeeId(this ClaimsPrincipal user) =>
        user.FindFirstValue("employeeId");
}
