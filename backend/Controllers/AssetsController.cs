using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,IT")]
[Route("api/assets")]
public sealed class AssetsController(
    ApplicationDbContext dbContext,
    AssetLifecycleService lifecycleService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AssetDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAll(CancellationToken cancellationToken)
    {
        var assets = await dbContext.Assets
            .AsNoTracking()
            .OrderBy(asset => asset.AssetCode)
            .Select(asset => new AssetDto(
                asset.Id,
                asset.AssetCode,
                asset.Category,
                asset.Brand,
                asset.Model,
                asset.SerialNumber,
                asset.Status,
                asset.Location,
                asset.PurchaseDate,
                asset.WarrantyEndDate,
                asset.Assignments.Where(x => x.ReturnedAt == null).Select(x => x.EmployeeId).FirstOrDefault(),
                asset.Assignments.Where(x => x.ReturnedAt == null).Select(x => x.Employee.FullName).FirstOrDefault(),
                asset.Assignments.Where(x => x.ReturnedAt == null).Select(x => x.Employee.Department).FirstOrDefault(),
                asset.Assignments.Where(x => x.ReturnedAt == null).Select(x => (DateTimeOffset?)x.AssignedAt).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Ok(assets);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Assets
            .AsNoTracking()
            .Where(current => current.Id == id)
            .Select(current => new AssetDto(current.Id, current.AssetCode, current.Category, current.Brand, current.Model,
                current.SerialNumber, current.Status, current.Location, current.PurchaseDate, current.WarrantyEndDate,
                current.Assignments.Where(x => x.ReturnedAt == null).Select(x => x.EmployeeId).FirstOrDefault(),
                current.Assignments.Where(x => x.ReturnedAt == null).Select(x => x.Employee.FullName).FirstOrDefault(),
                current.Assignments.Where(x => x.ReturnedAt == null).Select(x => x.Employee.Department).FirstOrDefault(),
                current.Assignments.Where(x => x.ReturnedAt == null).Select(x => (DateTimeOffset?)x.AssignedAt).FirstOrDefault()))
            .FirstOrDefaultAsync(cancellationToken);

        return asset is null ? NotFound() : Ok(asset);
    }

    [HttpPost]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    [ProducesResponseType<AssetDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Create(
        AssetCreateDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId is null) return Unauthorized();

        if (!request.Status.Trim().Equals(AssetLifecycleRules.Available, StringComparison.OrdinalIgnoreCase))
        {
            return AssignmentStatusConflict(
                "Yeni cihaz yalnız Boşta durumunda oluşturulabilir. Diğer durumlar ilgili yaşam döngüsü işlemleriyle belirlenir.");
        }

        var assetCode = request.AssetCode.Trim();
        var serialNumber = request.SerialNumber.Trim();

        var duplicateResult = await GetDuplicateResult(
            assetCode,
            serialNumber,
            excludedId: null,
            cancellationToken);

        if (duplicateResult is not null)
        {
            return duplicateResult;
        }

        var asset = new Entities.Asset
        {
            Id = Guid.NewGuid().ToString("N"),
            AssetCode = assetCode,
            Category = request.Category.Trim(),
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            SerialNumber = serialNumber,
            Status = AssetLifecycleRules.Available,
            Location = request.Location.Trim(),
            PurchaseDate = request.PurchaseDate!.Value,
            WarrantyEndDate = request.WarrantyEndDate!.Value
        };

        dbContext.Assets.Add(asset);
        dbContext.AssetMovements.Add(AssetMovementFactory.Create(
            asset.Id,
            AssetMovementType.InventoryCreated,
            DateTimeOffset.UtcNow,
            null,
            asset.Status,
            currentUserId,
            "Cihaz envantere eklendi."));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return DuplicateConflict("Varlık kodu veya seri numarası başka bir cihazda kullanılıyor.");
        }

        var response = ToDto(asset);
        return CreatedAtAction(nameof(GetById), new { id = asset.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Update(
        string id,
        AssetUpdateDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId is null) return Unauthorized();

        var asset = await dbContext.Assets
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (asset is null)
        {
            return NotFound();
        }

        var requestedStatus = request.Status.Trim();
        if (!AssetLifecycleRules.ValidStatuses.Contains(requestedStatus))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz cihaz durumu",
                Detail = "Cihaz durumu Boşta, Zimmetli, Bakımda, Kayıp, Hurda veya Elden Çıkarıldı olmalıdır."
            });
        }

        var statusChanged = !asset.Status.Equals(requestedStatus, StringComparison.OrdinalIgnoreCase);
        if (statusChanged && AssetLifecycleRules.IsCritical(requestedStatus))
        {
            return AssignmentStatusConflict(
                "Kayıp, Hurda ve Elden Çıkarıldı durumları yalnız ilgili durum işlemi üzerinden kaydedilebilir.");
        }

        var hasActiveAssignment = await dbContext.Assignments.AnyAsync(
            assignment => assignment.AssetId == id && assignment.ReturnedAt == null,
            cancellationToken);
        var requestedStatusIsAssigned = requestedStatus.Equals(
            AssetLifecycleRules.Assigned,
            StringComparison.OrdinalIgnoreCase);

        if (hasActiveAssignment != requestedStatusIsAssigned)
        {
            return AssignmentStatusConflict(
                hasActiveAssignment
                    ? "Aktif zimmeti bulunan cihazın durumu zimmet iadesi tamamlanmadan değiştirilemez."
                    : "Cihaz Zimmetli durumuna yalnız yeni zimmet işlemiyle geçirilebilir.");
        }

        var assetCode = request.AssetCode.Trim();
        var serialNumber = request.SerialNumber.Trim();

        var duplicateResult = await GetDuplicateResult(
            assetCode,
            serialNumber,
            id,
            cancellationToken);

        if (duplicateResult is not null)
        {
            return duplicateResult;
        }

        var previousStatus = asset.Status;
        asset.AssetCode = assetCode;
        asset.Category = request.Category.Trim();
        asset.Brand = request.Brand.Trim();
        asset.Model = request.Model.Trim();
        asset.SerialNumber = serialNumber;
        asset.Status = requestedStatus;
        asset.Location = request.Location.Trim();
        asset.PurchaseDate = request.PurchaseDate!.Value;
        asset.WarrantyEndDate = request.WarrantyEndDate!.Value;

        var movementType = statusChanged
            ? requestedStatus.Equals(AssetLifecycleRules.InMaintenance, StringComparison.OrdinalIgnoreCase)
                ? AssetMovementType.MaintenanceStarted
                : previousStatus.Equals(AssetLifecycleRules.InMaintenance, StringComparison.OrdinalIgnoreCase) &&
                  requestedStatus.Equals(AssetLifecycleRules.Available, StringComparison.OrdinalIgnoreCase)
                    ? AssetMovementType.MaintenanceCompleted
                    : AssetMovementType.StatusChanged
            : AssetMovementType.InformationUpdated;
        dbContext.AssetMovements.Add(AssetMovementFactory.Create(
            asset.Id,
            movementType,
            DateTimeOffset.UtcNow,
            previousStatus,
            requestedStatus,
            currentUserId,
            statusChanged ? $"Cihaz durumu {previousStatus} durumundan {requestedStatus} durumuna geçirildi." : "Cihaz bilgileri güncellendi."));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return DuplicateConflict("Varlık kodu veya seri numarası başka bir cihazda kullanılıyor.");
        }

        return Ok(ToDto(asset));
    }

    [HttpGet("{id}/movements")]
    [ProducesResponseType<IReadOnlyList<AssetMovementDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AssetMovementDto>>> GetMovements(
        string id,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Assets.AsNoTracking().AnyAsync(asset => asset.Id == id, cancellationToken))
            return NotFound();

        return Ok(await lifecycleService.GetMovementsAsync(id, cancellationToken));
    }

    [HttpPost("{id}/mark-lost")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public Task<ActionResult<AssetMovementDto>> MarkLost(
        string id,
        AssetLostDto request,
        CancellationToken cancellationToken) =>
        CompleteLifecycleOperation(
            userId => lifecycleService.MarkLostAsync(id, request, userId, cancellationToken));

    [HttpPost("{id}/scrap")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public Task<ActionResult<AssetMovementDto>> Scrap(
        string id,
        AssetScrapDto request,
        CancellationToken cancellationToken) =>
        CompleteLifecycleOperation(
            userId => lifecycleService.ScrapAsync(id, request, userId, cancellationToken));

    [HttpPost("{id}/dispose")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public Task<ActionResult<AssetMovementDto>> Dispose(
        string id,
        AssetDisposeDto request,
        CancellationToken cancellationToken) =>
        CompleteLifecycleOperation(
            userId => lifecycleService.DisposeAsync(id, request, userId, cancellationToken));

    private async Task<ActionResult<AssetMovementDto>> CompleteLifecycleOperation(
        Func<string, Task<AssetLifecycleOperationResult>> operation)
    {
        var currentUserId = User.GetUserId();
        if (currentUserId is null) return Unauthorized();

        var result = await operation(currentUserId);
        return result.Status switch
        {
            AssetLifecycleOperationStatus.Success => Ok(result.Movement),
            AssetLifecycleOperationStatus.NotFound => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Cihaz bulunamadı",
                Detail = result.ErrorMessage
            }),
            AssetLifecycleOperationStatus.Conflict => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Cihaz durumu değiştirilemedi",
                Detail = result.ErrorMessage
            }),
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Geçersiz yaşam döngüsü işlemi",
                Detail = result.ErrorMessage
            })
        };
    }

    private async Task<ActionResult<AssetDto>?> GetDuplicateResult(
        string assetCode,
        string serialNumber,
        string? excludedId,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Assets.AnyAsync(
                asset => asset.Id != excludedId && asset.AssetCode == assetCode,
                cancellationToken))
        {
            return DuplicateConflict("Bu varlık kodu başka bir cihazda kullanılıyor.");
        }

        if (await dbContext.Assets.AnyAsync(
                asset => asset.Id != excludedId && asset.SerialNumber == serialNumber,
                cancellationToken))
        {
            return DuplicateConflict("Bu seri numarası başka bir cihazda kullanılıyor.");
        }

        return null;
    }

    private ConflictObjectResult DuplicateConflict(string detail) =>
        Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Tekrarlanan cihaz bilgisi",
            Detail = detail
        });

    private ConflictObjectResult AssignmentStatusConflict(string detail) =>
        Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Cihaz ve zimmet durumu uyuşmuyor",
            Detail = detail
        });

    private static AssetDto ToDto(Entities.Asset asset) =>
        new(
            asset.Id,
            asset.AssetCode,
            asset.Category,
            asset.Brand,
            asset.Model,
            asset.SerialNumber,
            asset.Status,
            asset.Location,
            asset.PurchaseDate,
            asset.WarrantyEndDate,
            null, null, null, null);
}
