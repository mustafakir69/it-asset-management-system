using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed record UserDto(
    string Id,
    string? EmployeeId,
    string FullName,
    string? Department,
    string? EmployeeNo,
    string Username,
    string Email,
    string Role,
    string RoleDisplayName,
    bool IsActive,
    string Status);

public sealed record UsernameSuggestionDto(string Username);

public sealed class UserCreateDto
{
    public string? EmployeeId { get; init; }

    [StringLength(100, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-100 karakter arasında olmalıdır.")]
    public string? Username { get; init; }

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [StringLength(254, ErrorMessage = "E-posta adresi en fazla 254 karakter olabilir.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Parola zorunludur.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Parola en az 8, en fazla 128 karakter olmalıdır.")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Rol zorunludur.")]
    public string Role { get; init; } = string.Empty;
}
