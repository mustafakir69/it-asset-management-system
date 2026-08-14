using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed record LoginRequestDto(
    [param: Required(ErrorMessage = "Kullanıcı adı veya e-posta zorunludur.")]
    string Identifier,
    [param: Required(ErrorMessage = "Parola zorunludur.")]
    string Password);

public sealed record AuthUserDto(
    string Id,
    string? EmployeeId,
    string Username,
    string Email,
    string Role,
    string RoleDisplayName);

public sealed record LoginResponseDto(
    string Token,
    DateTimeOffset ExpiresAt,
    AuthUserDto User);
