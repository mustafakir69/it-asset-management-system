namespace TakipProgrami.Api.DTOs;

public sealed record EmployeeDto(
    string Id,
    string EmployeeNo,
    string FullName,
    string Department,
    string Email);
