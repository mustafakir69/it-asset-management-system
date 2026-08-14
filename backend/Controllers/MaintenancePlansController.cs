using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Route("api/maintenance/plans")]
public sealed class MaintenancePlansController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MaintenancePlanDto>>> GetAll(CancellationToken cancellationToken)
    {
        var plans = await dbContext.MaintenancePlans.AsNoTracking().Include(plan => plan.Asset)
            .OrderByDescending(plan => plan.IsActive).ThenBy(plan => plan.StartDate)
            .ToListAsync(cancellationToken);
        return Ok(plans.Select(ToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenancePlanDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var plan = await dbContext.MaintenancePlans.AsNoTracking().Include(item => item.Asset)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return plan is null ? NotFound() : Ok(ToDto(plan));
    }

    [HttpPost]
    public async Task<ActionResult<MaintenancePlanDto>> Create(
        MaintenancePlanCreateDto request,
        CancellationToken cancellationToken)
    {
        var asset = await GetEligibleAsset(request.AssetId, cancellationToken);
        if (asset is null) return ValidationProblem(ModelState);

        var createdAt = DateTimeOffset.UtcNow;
        var plan = new MaintenancePlan
        {
            Id = Guid.NewGuid().ToString("N"), AssetId = asset.Id, Name = request.Name.Trim(),
            Description = Clean(request.Description), FrequencyDays = request.FrequencyDays,
            StartDate = request.StartDate!.Value,
            ResponsibleTechnician = request.ResponsibleTechnician.Trim(), IsActive = true,
            CreatedAt = createdAt, Asset = asset
        };
        var firstTask = new MaintenanceTask
        {
            Id = Guid.NewGuid().ToString("N"), MaintenancePlanId = plan.Id, AssetId = asset.Id,
            Title = plan.Name, Description = plan.Description, PlannedDate = plan.StartDate,
            Status = MaintenanceTaskStatus.Planned, TechnicianName = plan.ResponsibleTechnician,
            CreatedAt = createdAt
        };
        dbContext.MaintenancePlans.Add(plan);
        dbContext.MaintenanceTasks.Add(firstTask);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, ToDto(plan));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MaintenancePlanDto>> Update(
        string id,
        MaintenancePlanUpdateDto request,
        CancellationToken cancellationToken)
    {
        var plan = await dbContext.MaintenancePlans.Include(item => item.Asset)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (plan is null) return NotFound();

        var asset = await GetEligibleAsset(request.AssetId, cancellationToken);
        if (asset is null) return ValidationProblem(ModelState);

        plan.AssetId = asset.Id;
        plan.Asset = asset;
        plan.Name = request.Name.Trim();
        plan.Description = Clean(request.Description);
        plan.FrequencyDays = request.FrequencyDays;
        plan.StartDate = request.StartDate!.Value;
        plan.ResponsibleTechnician = request.ResponsibleTechnician.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(plan));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<MaintenancePlanDto>> UpdateStatus(
        string id,
        MaintenancePlanStatusDto request,
        CancellationToken cancellationToken)
    {
        var plan = await dbContext.MaintenancePlans.Include(item => item.Asset)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (plan is null) return NotFound();
        plan.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(plan));
    }

    private async Task<Asset?> GetEligibleAsset(string assetId, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Assets.FirstOrDefaultAsync(item => item.Id == assetId, cancellationToken);
        if (asset is null)
        {
            ModelState.AddModelError(nameof(MaintenancePlanCreateDto.AssetId), "Seçilen cihaz bulunamadı.");
            return null;
        }
        if (asset.Status is "Hurda" or "Elden çıkarıldı")
        {
            ModelState.AddModelError(nameof(MaintenancePlanCreateDto.AssetId), "Hurda veya elden çıkarılmış cihazlar için bakım planı oluşturulamaz.");
            return null;
        }
        return asset;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static MaintenancePlanDto ToDto(MaintenancePlan plan) => new(
        plan.Id, plan.AssetId, plan.Asset.AssetCode, $"{plan.Asset.Brand} {plan.Asset.Model}",
        plan.Name, plan.Description, plan.FrequencyDays, plan.StartDate,
        plan.ResponsibleTechnician, plan.IsActive, plan.CreatedAt);
}
