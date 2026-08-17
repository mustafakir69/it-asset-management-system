using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,IT")]
[Route("api/maintenance/tasks")]
public sealed class MaintenanceTasksController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MaintenanceTaskDto>>> GetAll(CancellationToken cancellationToken)
    {
        var tasks = await dbContext.MaintenanceTasks.AsNoTracking().Include(task => task.Asset).Include(task => task.MaintenancePlan).ThenInclude(x => x.ResponsibleUser).ThenInclude(x => x.Employee).Include(task => task.CompletedByUser).ThenInclude(x => x!.Employee)
            .OrderBy(task => task.PlannedDate).ToListAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.Today);
        return Ok(tasks.Select(task => ToDto(task, today)).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenanceTaskDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var task = await dbContext.MaintenanceTasks.AsNoTracking().Include(item => item.Asset).Include(item => item.MaintenancePlan).ThenInclude(x => x.ResponsibleUser).ThenInclude(x => x.Employee).Include(item => item.CompletedByUser).ThenInclude(x => x!.Employee)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return task is null ? NotFound() : Ok(ToDto(task, DateOnly.FromDateTime(DateTime.Today)));
    }

    [HttpPut("{id}/complete")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<MaintenanceTaskDto>> Complete(
        string id,
        MaintenanceTaskCompleteDto request,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.MaintenanceTasks.Include(item => item.Asset).Include(item => item.MaintenancePlan)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (task is null) return NotFound();
        if (task.Status != MaintenanceTaskStatus.Planned)
            return StateConflict(task.Status == MaintenanceTaskStatus.Completed ? "Bu görev zaten tamamlanmış." : "İptal edilmiş görev tamamlanamaz.");

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        task.Status = MaintenanceTaskStatus.Completed;
        task.CompletedDate = request.CompletedDate!.Value;
        task.CompletedByUserId = User.GetUserId();
        task.Result = request.Result.Trim();
        task.WorkNotes = request.WorkNotes.Trim();

        if (task.MaintenancePlan.IsActive)
        {
            var nextDate = MaintenanceRules.GetNextPlannedDate(
                task.PlannedDate,
                task.MaintenancePlan.FrequencyDays);
            var exists = await dbContext.MaintenanceTasks.AnyAsync(
                item => item.MaintenancePlanId == task.MaintenancePlanId && item.PlannedDate == nextDate,
                cancellationToken);
            if (!exists)
            {
                dbContext.MaintenanceTasks.Add(new MaintenanceTask
                {
                    Id = Guid.NewGuid().ToString("N"), MaintenancePlanId = task.MaintenancePlanId,
                    AssetId = task.AssetId, Title = task.MaintenancePlan.Name,
                    Description = task.MaintenancePlan.Description, PlannedDate = nextDate,
                    Status = MaintenanceTaskStatus.Planned,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Ok(await GetDto(task.Id, cancellationToken));
    }

    [HttpPut("{id}/cancel")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<MaintenanceTaskDto>> Cancel(
        string id,
        MaintenanceTaskCancelDto request,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.MaintenanceTasks.Include(item => item.Asset).Include(item => item.MaintenancePlan).ThenInclude(x => x.ResponsibleUser).ThenInclude(x => x.Employee).Include(item => item.CompletedByUser).ThenInclude(x => x!.Employee)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (task is null) return NotFound();
        if (task.Status != MaintenanceTaskStatus.Planned)
            return StateConflict(task.Status == MaintenanceTaskStatus.Completed ? "Tamamlanmış görev iptal edilemez." : "Bu görev zaten iptal edilmiş.");
        task.Status = MaintenanceTaskStatus.Cancelled;
        task.CancellationReason = request.CancellationReason.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(task, DateOnly.FromDateTime(DateTime.Today)));
    }

    [HttpPut("{id}/reschedule")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<MaintenanceTaskDto>> Reschedule(
        string id,
        MaintenanceTaskRescheduleDto request,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.MaintenanceTasks.Include(item => item.Asset).Include(item => item.MaintenancePlan).ThenInclude(x => x.ResponsibleUser).ThenInclude(x => x.Employee).Include(item => item.CompletedByUser).ThenInclude(x => x!.Employee)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (task is null) return NotFound();
        if (task.Status != MaintenanceTaskStatus.Planned)
            return StateConflict("Tamamlanmış veya iptal edilmiş görev yeniden planlanamaz.");
        var newDate = request.PlannedDate!.Value;
        if (await dbContext.MaintenanceTasks.AnyAsync(
                item => item.Id != id && item.MaintenancePlanId == task.MaintenancePlanId && item.PlannedDate == newDate,
                cancellationToken))
            return StateConflict("Bu plan için seçilen tarihte zaten bir bakım görevi var.");
        task.PlannedDate = newDate;
        task.WorkNotes = string.IsNullOrWhiteSpace(task.WorkNotes)
            ? request.WorkNotes.Trim()
            : $"{task.WorkNotes}\nYeniden planlama: {request.WorkNotes.Trim()}";
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(task, DateOnly.FromDateTime(DateTime.Today)));
    }

    private ConflictObjectResult StateConflict(string detail) => Conflict(new ProblemDetails
    {
        Status = StatusCodes.Status409Conflict, Title = "Bakım görevi durumu uygun değil", Detail = detail
    });

    internal static MaintenanceTaskDto ToDto(MaintenanceTask task, DateOnly today) => new(
        task.Id, task.MaintenancePlanId, task.AssetId, task.Asset.AssetCode,
        $"{task.Asset.Brand} {task.Asset.Model}", task.Title, task.Description,
        task.PlannedDate, task.CompletedDate, StoredStatus(task.Status), DisplayStatus(task, today),
        task.MaintenancePlan.ResponsibleUserId,
        task.MaintenancePlan.ResponsibleUser.Employee?.FullName ?? task.MaintenancePlan.ResponsibleUser.Username,
        task.Notes, task.CompletedByUserId,
        task.CompletedByUser == null ? null : task.CompletedByUser.Employee?.FullName ?? task.CompletedByUser.Username,
        task.Result, task.WorkNotes,
        task.CancellationReason, task.CreatedAt);

    private async Task<MaintenanceTaskDto> GetDto(string id, CancellationToken ct) =>
        ToDto(await dbContext.MaintenanceTasks.AsNoTracking().Include(x => x.Asset).Include(x => x.MaintenancePlan).ThenInclude(x => x.ResponsibleUser).ThenInclude(x => x.Employee).Include(x => x.CompletedByUser).ThenInclude(x => x!.Employee).FirstAsync(x => x.Id == id, ct), DateOnly.FromDateTime(DateTime.Today));

    private static string StoredStatus(MaintenanceTaskStatus status) => status switch
    {
        MaintenanceTaskStatus.Completed => "Tamamlandı",
        MaintenanceTaskStatus.Cancelled => "İptal Edildi",
        _ => "Planlandı"
    };

    private static string DisplayStatus(MaintenanceTask task, DateOnly today)
    {
        if (task.Status == MaintenanceTaskStatus.Completed) return "Tamamlandı";
        if (task.Status == MaintenanceTaskStatus.Cancelled) return "İptal Edildi";
        if (task.PlannedDate < today) return "Gecikti";
        return task.PlannedDate.DayNumber - today.DayNumber <= 7 ? "Yaklaşıyor" : "Planlandı";
    }
}
