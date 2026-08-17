using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,IT")]
[Route("api/assets")]
public sealed class AssetsController(ApplicationDbContext dbContext) : ControllerBase
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
        if (request.Status.Trim().Equals("Zimmetli", StringComparison.OrdinalIgnoreCase))
        {
            return AssignmentStatusConflict(
                "Yeni cihaz Zimmetli durumunda oluşturulamaz. Zimmet durumu yalnız zimmet işlemiyle belirlenir.");
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
            Status = request.Status.Trim(),
            Location = request.Location.Trim(),
            PurchaseDate = request.PurchaseDate!.Value,
            WarrantyEndDate = request.WarrantyEndDate!.Value
        };

        dbContext.Assets.Add(asset);

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
        var asset = await dbContext.Assets
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (asset is null)
        {
            return NotFound();
        }

        var hasActiveAssignment = await dbContext.Assignments.AnyAsync(
            assignment => assignment.AssetId == id && assignment.ReturnedAt == null,
            cancellationToken);
        var requestedStatusIsAssigned = request.Status.Trim().Equals(
            "Zimmetli",
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

        asset.AssetCode = assetCode;
        asset.Category = request.Category.Trim();
        asset.Brand = request.Brand.Trim();
        asset.Model = request.Model.Trim();
        asset.SerialNumber = serialNumber;
        asset.Status = request.Status.Trim();
        asset.Location = request.Location.Trim();
        asset.PurchaseDate = request.PurchaseDate!.Value;
        asset.WarrantyEndDate = request.WarrantyEndDate!.Value;

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
