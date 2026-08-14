using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/warranties")]
public sealed class WarrantiesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<WarrantyAssetDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarrantyAssetDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var assets = await dbContext.Assets
            .AsNoTracking()
            .OrderBy(asset => asset.AssetCode)
            .Select(asset => new
            {
                asset.Id,
                asset.AssetCode,
                asset.Category,
                asset.Brand,
                asset.Model,
                asset.SerialNumber,
                asset.Location,
                asset.PurchaseDate,
                asset.WarrantyEndDate
            })
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var warranties = assets
            .Select(asset => CreateWarrantyDto(
                asset.Id,
                asset.AssetCode,
                asset.Category,
                asset.Brand,
                asset.Model,
                asset.SerialNumber,
                asset.Location,
                asset.PurchaseDate,
                asset.WarrantyEndDate,
                today))
            .ToList();

        return Ok(warranties);
    }

    private static WarrantyAssetDto CreateWarrantyDto(
        string assetId,
        string assetCode,
        string category,
        string brand,
        string model,
        string serialNumber,
        string location,
        DateOnly purchaseDate,
        DateOnly? warrantyEndDate,
        DateOnly today)
    {
        var calculation = WarrantyRules.Calculate(warrantyEndDate, today);

        return new WarrantyAssetDto(
            assetId,
            assetCode,
            category,
            brand,
            model,
            serialNumber,
            location,
            purchaseDate,
            warrantyEndDate,
            calculation.RemainingDays,
            calculation.Status);
    }
}
