using System.ComponentModel.DataAnnotations;

namespace TakipProgrami.Api.DTOs;

public sealed record AssetMovementDto(
    string Id,
    string AssetId,
    string MovementType,
    DateTimeOffset OccurredAt,
    string? PreviousStatus,
    string NewStatus,
    string PerformedByUserId,
    string PerformedByName,
    string? Description,
    string? Reason,
    string? Method,
    string? RelatedEntityType,
    string? RelatedEntityId);

public sealed class AssetLostDto
{
    [Required(ErrorMessage = "Kayıp tarihi zorunludur.")]
    public DateOnly? LostDate { get; init; }

    [Required(ErrorMessage = "Kayıp açıklaması zorunludur.")]
    [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    public string Description { get; init; } = string.Empty;
}

public sealed class AssetScrapDto
{
    [Required(ErrorMessage = "Hurdaya ayrılma tarihi zorunludur.")]
    public DateOnly? ScrappedDate { get; init; }

    [Required(ErrorMessage = "Hurda nedeni zorunludur.")]
    [StringLength(250, ErrorMessage = "Hurda nedeni en fazla 250 karakter olabilir.")]
    public string Reason { get; init; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    public string? Description { get; init; }
}

public sealed class AssetDisposeDto
{
    [Required(ErrorMessage = "Elden çıkarma tarihi zorunludur.")]
    public DateOnly? DisposedDate { get; init; }

    [Required(ErrorMessage = "Elden çıkarma yöntemi zorunludur.")]
    [StringLength(100, ErrorMessage = "Yöntem en fazla 100 karakter olabilir.")]
    public string Method { get; init; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    public string? Description { get; init; }
}
