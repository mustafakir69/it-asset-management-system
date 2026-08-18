namespace TakipProgrami.Api.DTOs;

public sealed record GlobalSearchResultDto(
    string Category,
    string Title,
    string Description,
    string Route);
