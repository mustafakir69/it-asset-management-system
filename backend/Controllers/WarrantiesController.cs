using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,IT")]
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
                ,asset.Status,
                CurrentAssigneeEmployeeId = asset.Assignments.Where(x => x.ReturnedAt == null).Select(x => x.EmployeeId).FirstOrDefault(),
                CurrentAssigneeName = asset.Assignments.Where(x => x.ReturnedAt == null).Select(x => x.Employee.FullName).FirstOrDefault(),
                CurrentAssigneeDepartment = asset.Assignments.Where(x => x.ReturnedAt == null).Select(x => x.Employee.Department).FirstOrDefault()
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
                asset.Status,
                asset.CurrentAssigneeEmployeeId,
                asset.CurrentAssigneeName,
                asset.CurrentAssigneeDepartment,
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
        string assetStatus,
        string? currentAssigneeEmployeeId,
        string? currentAssigneeName,
        string? currentAssigneeDepartment,
        DateOnly today)
    {
        var calculation = WarrantyRules.Calculate(warrantyEndDate, today);

        return new WarrantyAssetDto(
            assetId,
            assetCode,
            brand + " " + model,
            category,
            brand,
            model,
            serialNumber,
            location,
            purchaseDate,
            warrantyEndDate,
            calculation.RemainingDays,
            calculation.Status,
            assetStatus,
            currentAssigneeEmployeeId,
            currentAssigneeName,
            currentAssigneeDepartment);
    }
}
