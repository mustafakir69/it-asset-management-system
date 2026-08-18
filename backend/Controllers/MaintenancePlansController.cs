using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Controllers;

[ApiController, Authorize(Roles = "Admin,IT"), Route("api/maintenance/plans")]
public sealed class MaintenancePlansController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<MaintenancePlanDto>>> GetAll(CancellationToken ct) =>
        Ok(await Query().OrderByDescending(x => x.IsActive).ThenBy(x => x.NextDueAt).Select(ToDto()).ToListAsync(ct));

    [HttpGet("{id}")] public async Task<ActionResult<MaintenancePlanDto>> GetById(string id, CancellationToken ct)
    { var value = await Query().Where(x => x.Id == id).Select(ToDto()).FirstOrDefaultAsync(ct); return value is null ? NotFound() : Ok(value); }

    [HttpPost] public async Task<ActionResult<MaintenancePlanDto>> Create(MaintenancePlanCreateDto input, CancellationToken ct)
    {
        var validation = await Validate(input.AssetId, input.ResponsibleUserId, ct); if (validation is not null) return validation;
        var now = DateTimeOffset.UtcNow;
        var plan = new MaintenancePlan { Id = Guid.NewGuid().ToString("N"), AssetId = input.AssetId, Name = input.Name.Trim(), Description = Clean(input.Description),
            FrequencyDays = input.FrequencyDays, StartDate = input.StartDate!.Value, ResponsibleUserId = input.ResponsibleUserId,
            EstimatedDurationMinutes = input.EstimatedDurationMinutes, ReminderLeadDays = input.ReminderLeadDays, NextDueAt = input.StartDate.Value, IsActive = true, CreatedAt = now };
        db.MaintenancePlans.Add(plan);
        db.MaintenanceTasks.Add(new MaintenanceTask { Id = Guid.NewGuid().ToString("N"), MaintenancePlanId = plan.Id, AssetId = plan.AssetId,
            Title = plan.Name, Description = plan.Description, PlannedDate = plan.StartDate, Status = MaintenanceTaskStatus.Planned, CreatedAt = now });
        await db.SaveChangesAsync(ct);
        var dto = await Query().Where(x => x.Id == plan.Id).Select(ToDto()).FirstAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, dto);
    }

    [HttpPut("{id}")] public async Task<ActionResult<MaintenancePlanDto>> Update(string id, MaintenancePlanUpdateDto input, CancellationToken ct)
    {
        var plan = await db.MaintenancePlans.FirstOrDefaultAsync(x => x.Id == id, ct); if (plan is null) return NotFound();
        var validation = await Validate(input.AssetId, input.ResponsibleUserId, ct); if (validation is not null) return validation;
        plan.AssetId = input.AssetId; plan.Name = input.Name.Trim(); plan.Description = Clean(input.Description); plan.FrequencyDays = input.FrequencyDays;
        plan.StartDate = input.StartDate!.Value; plan.ResponsibleUserId = input.ResponsibleUserId; plan.EstimatedDurationMinutes = input.EstimatedDurationMinutes;
        plan.ReminderLeadDays = input.ReminderLeadDays; if (plan.NextDueAt < plan.StartDate) plan.NextDueAt = plan.StartDate;
        await db.SaveChangesAsync(ct); return Ok(await Query().Where(x => x.Id == id).Select(ToDto()).FirstAsync(ct));
    }

    [HttpPut("{id}/status")] public async Task<ActionResult<MaintenancePlanDto>> Status(string id, MaintenancePlanStatusDto input, CancellationToken ct)
    { var plan = await db.MaintenancePlans.FirstOrDefaultAsync(x => x.Id == id, ct); if (plan is null) return NotFound(); plan.IsActive = input.IsActive; await db.SaveChangesAsync(ct); return Ok(await Query().Where(x => x.Id == id).Select(ToDto()).FirstAsync(ct)); }

    private async Task<ActionResult?> Validate(string assetId, string userId, CancellationToken ct)
    {
        var asset = await db.Assets.FirstOrDefaultAsync(x => x.Id == assetId, ct);
        if (asset is null || asset.Status is "Hurda" or "Elden Çıkarıldı") return BadRequest(new ProblemDetails { Detail = "Geçerli bir cihaz seçin." });
        var user = await db.AppUsers.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null || !user.IsActive || user.Role != AppRole.IT) return BadRequest(new ProblemDetails { Detail = "Sorumlu yalnızca aktif bir IT kullanıcısı olabilir." });
        return null;
    }
    private IQueryable<MaintenancePlan> Query() => db.MaintenancePlans.AsNoTracking();
    private static System.Linq.Expressions.Expression<Func<MaintenancePlan, MaintenancePlanDto>> ToDto() => x => new(x.Id, x.AssetId, x.Asset.AssetCode,
        x.Asset.Brand + " " + x.Asset.Model, x.Name, x.Description, x.FrequencyDays, x.StartDate, x.ResponsibleUserId,
        x.ResponsibleUser.Employee != null ? x.ResponsibleUser.Employee.FullName : x.ResponsibleUser.Username,
        x.EstimatedDurationMinutes, x.ReminderLeadDays, x.NextDueAt, x.IsActive, x.CreatedAt);
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
