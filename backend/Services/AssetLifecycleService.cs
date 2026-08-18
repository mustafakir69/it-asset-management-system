using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Services;

public enum AssetLifecycleOperationStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

public sealed record AssetLifecycleOperationResult(
    AssetLifecycleOperationStatus Status,
    AssetMovementDto? Movement = null,
    string? ErrorMessage = null);

public sealed class AssetLifecycleService(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<AssetMovementDto>> GetMovementsAsync(
        string assetId,
        CancellationToken cancellationToken)
    {
        var movements = await dbContext.AssetMovements
            .AsNoTracking()
            .Where(item => item.AssetId == assetId)
            .Include(item => item.PerformedByUser)
                .ThenInclude(user => user.Employee)
            .OrderByDescending(item => item.OccurredAt)
            .ThenByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        return movements.Select(ToDto).ToList();
    }

    public Task<AssetLifecycleOperationResult> MarkLostAsync(
        string assetId,
        AssetLostDto request,
        string currentUserId,
        CancellationToken cancellationToken) =>
        TransitionAsync(
            assetId,
            AssetLifecycleRules.Lost,
            AssetMovementType.MarkedLost,
            request.LostDate!.Value,
            currentUserId,
            request.Description,
            null,
            null,
            cancellationToken);

    public Task<AssetLifecycleOperationResult> ScrapAsync(
        string assetId,
        AssetScrapDto request,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var reason = request.Reason.Trim();
        if (!AssetLifecycleRules.ScrapReasons.Contains(reason))
        {
            return Task.FromResult(Invalid(
                "Hurda nedeni izin verilen değerlerden biri olmalıdır."));
        }

        return TransitionAsync(
            assetId,
            AssetLifecycleRules.Scrapped,
            AssetMovementType.Scrapped,
            request.ScrappedDate!.Value,
            currentUserId,
            request.Description,
            reason,
            null,
            cancellationToken);
    }

    public Task<AssetLifecycleOperationResult> DisposeAsync(
        string assetId,
        AssetDisposeDto request,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var method = request.Method.Trim();
        if (!AssetLifecycleRules.DisposalMethods.Contains(method))
        {
            return Task.FromResult(Invalid(
                "Elden çıkarma yöntemi izin verilen değerlerden biri olmalıdır."));
        }

        return TransitionAsync(
            assetId,
            AssetLifecycleRules.Disposed,
            AssetMovementType.Disposed,
            request.DisposedDate!.Value,
            currentUserId,
            request.Description,
            null,
            method,
            cancellationToken);
    }

    private async Task<AssetLifecycleOperationResult> TransitionAsync(
        string assetId,
        string newStatus,
        AssetMovementType movementType,
        DateOnly occurredDate,
        string currentUserId,
        string? description,
        string? reason,
        string? method,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            cancellationToken);
        var asset = await dbContext.Assets.FirstOrDefaultAsync(
            item => item.Id == assetId,
            cancellationToken);
        if (asset is null)
        {
            return new(AssetLifecycleOperationStatus.NotFound, ErrorMessage: "Cihaz bulunamadı.");
        }

        if (await dbContext.Assignments.AnyAsync(
                assignment => assignment.AssetId == assetId && assignment.ReturnedAt == null,
                cancellationToken))
        {
            return Conflict(
                "Aktif zimmeti bulunan cihaz bu duruma geçirilemez. Önce zimmet iadesini tamamlayın.");
        }

        if (asset.Status.Equals(newStatus, StringComparison.OrdinalIgnoreCase))
        {
            return Conflict($"Cihaz zaten {newStatus} durumunda.");
        }

        var previousStatus = asset.Status;
        asset.Status = newStatus;
        var movement = AssetMovementFactory.Create(
            asset.Id,
            movementType,
            AtEventTime(occurredDate),
            previousStatus,
            newStatus,
            currentUserId,
            description,
            reason,
            method);
        dbContext.AssetMovements.Add(movement);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var response = await dbContext.AssetMovements
            .AsNoTracking()
            .Include(item => item.PerformedByUser)
                .ThenInclude(user => user.Employee)
            .FirstAsync(item => item.Id == movement.Id, cancellationToken);
        return new(AssetLifecycleOperationStatus.Success, ToDto(response));
    }

    private static DateTimeOffset AtEventTime(DateOnly date)
    {
        var currentTime = TimeOnly.FromTimeSpan(DateTimeOffset.Now.TimeOfDay);
        var localDate = date.ToDateTime(currentTime, DateTimeKind.Unspecified);
        return new DateTimeOffset(localDate, TimeZoneInfo.Local.GetUtcOffset(localDate));
    }

    private static AssetMovementDto ToDto(AssetMovement movement) => new(
        movement.Id,
        movement.AssetId,
        AssetLifecycleRules.MovementDisplayName(movement.MovementType),
        movement.OccurredAt,
        movement.PreviousStatus,
        movement.NewStatus,
        movement.PerformedByUserId,
        movement.PerformedByUser.Employee?.FullName ?? movement.PerformedByUser.Username,
        movement.Description,
        movement.Reason,
        movement.Method,
        movement.RelatedEntityType,
        movement.RelatedEntityId);

    private static AssetLifecycleOperationResult Invalid(string message) =>
        new(AssetLifecycleOperationStatus.ValidationError, ErrorMessage: message);

    private static AssetLifecycleOperationResult Conflict(string message) =>
        new(AssetLifecycleOperationStatus.Conflict, ErrorMessage: message);
}
