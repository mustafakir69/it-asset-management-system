using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;

namespace TakipProgrami.Api.Controllers;

[ApiController]
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
        var remainingDays = warrantyEndDate?.DayNumber - today.DayNumber;
        var warrantyStatus = remainingDays switch
        {
            null => "Garanti Bilgisi Yok",
            < 0 => "Süresi Doldu",
            <= 30 => "Yaklaşıyor",
            _ => "Aktif"
        };

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
            remainingDays,
            warrantyStatus);
    }
}
