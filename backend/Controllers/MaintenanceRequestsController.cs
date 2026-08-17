using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Controllers;

[ApiController, Authorize, Route("api/support-requests")]
public sealed class MaintenanceRequestsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Admin,IT")]
    public async Task<ActionResult<IReadOnlyList<MaintenanceRequestDto>>> GetAll(string? status, string? priority, string? search, CancellationToken ct)
    {
        var query = Query();
        if (!string.IsNullOrWhiteSpace(status)) { if (!TryStatus(status, out var parsed)) return Bad("Geçersiz destek talebi durumu."); query = query.Where(x => x.Status == parsed); }
        if (!string.IsNullOrWhiteSpace(priority)) { if (!TryPriority(priority, out var parsed)) return Bad("Geçersiz destek talebi önceliği."); query = query.Where(x => x.Priority == parsed); }
        if (!string.IsNullOrWhiteSpace(search)) { var s = search.Trim(); query = query.Where(x => x.Asset.AssetCode.Contains(s) || x.Title.Contains(s) || x.RequestedByEmployee.FullName.Contains(s)); }
        return Ok(await query.OrderByDescending(x => x.CreatedAt).Select(ToDto()).ToListAsync(ct));
    }

    [HttpGet("my"), Authorize(Roles = "Employee")]
    public async Task<ActionResult<IReadOnlyList<MaintenanceRequestDto>>> My(CancellationToken ct)
    { var employeeId = User.GetEmployeeId(); if (employeeId is null) return Forbid(); return Ok(await Query().Where(x => x.RequestedByEmployeeId == employeeId).OrderByDescending(x => x.CreatedAt).Select(ToDto()).ToListAsync(ct)); }

    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenanceRequestDto>> GetById(string id, CancellationToken ct)
    {
        var query = Query().Where(x => x.Id == id);
        if (User.IsInRole(nameof(AppRole.Employee))) { var employeeId = User.GetEmployeeId(); if (employeeId is null) return Forbid(); query = query.Where(x => x.RequestedByEmployeeId == employeeId); }
        var value = await query.Select(ToDto()).FirstOrDefaultAsync(ct); return value is null ? NotFound() : Ok(value);
    }

    [HttpPost, Authorize(Roles = "Employee")]
    public async Task<ActionResult<MaintenanceRequestDto>> Create(MaintenanceRequestCreateDto input, CancellationToken ct)
    {
        var employeeId = User.GetEmployeeId(); if (employeeId is null) return Forbid();
        if (!TryPriority(input.Priority, out var priority)) return Bad("Geçersiz destek talebi önceliği.");
        var ownsAsset = await db.Assignments.AnyAsync(x => x.AssetId == input.AssetId && x.EmployeeId == employeeId && x.ReturnedAt == null, ct);
        if (!ownsAsset) return StatusCode(403, new ProblemDetails { Status = 403, Title = "Yetkisiz cihaz", Detail = "Yalnızca size aktif zimmetli cihaz için destek talebi açabilirsiniz." });
        var now = DateTimeOffset.UtcNow;
        var request = new MaintenanceRequest { Id = Guid.NewGuid().ToString("N"), AssetId = input.AssetId, RequestedByEmployeeId = employeeId,
            Title = input.Title.Trim(), Description = input.Description.Trim(), Priority = priority, Status = MaintenanceRequestStatus.Open,
            CreatedAt = now, UpdatedAt = now };
        db.MaintenanceRequests.Add(request); await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, await Query().Where(x => x.Id == request.Id).Select(ToDto()).FirstAsync(ct));
    }

    [HttpPut("{id}/assign"), Authorize(Roles = "Admin,IT")]
    public async Task<ActionResult<MaintenanceRequestDto>> Assign(string id, MaintenanceRequestAssignDto input, CancellationToken ct)
    {
        var request = await db.MaintenanceRequests.FirstOrDefaultAsync(x => x.Id == id, ct); if (request is null) return NotFound();
        if (Terminal(request.Status)) return ConflictProblem("Tamamlanmış veya iptal edilmiş talep atanamaz.");
        var assignee = await db.AppUsers.FirstOrDefaultAsync(x => x.Id == input.AssignedToUserId && x.IsActive && x.Role == AppRole.IT, ct);
        if (assignee is null) return Bad("Talep yalnızca aktif bir IT kullanıcısına atanabilir.");
        request.AssignedToUserId = assignee.Id; request.Status = MaintenanceRequestStatus.Assigned; request.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct); return Ok(await Dto(id, ct));
    }

    [HttpPut("{id}/start"), Authorize(Roles = "Admin,IT")]
    public async Task<ActionResult<MaintenanceRequestDto>> Start(string id, CancellationToken ct)
    { var request = await db.MaintenanceRequests.FirstOrDefaultAsync(x => x.Id == id, ct); if (request is null) return NotFound(); if (request.Status != MaintenanceRequestStatus.Assigned) return ConflictProblem("Yalnızca atanmış talep işleme alınabilir."); request.Status = MaintenanceRequestStatus.InProgress; request.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return Ok(await Dto(id, ct)); }

    [HttpPut("{id}/complete"), Authorize(Roles = "Admin,IT")]
    public async Task<ActionResult<MaintenanceRequestDto>> Complete(string id, MaintenanceRequestCompleteDto input, CancellationToken ct)
    { var request = await db.MaintenanceRequests.FirstOrDefaultAsync(x => x.Id == id, ct); if (request is null) return NotFound(); if (request.Status != MaintenanceRequestStatus.InProgress) return ConflictProblem("Yalnızca işlemdeki talep tamamlanabilir."); request.Status = MaintenanceRequestStatus.Completed; request.CompletedAt = input.CompletedAt!.Value; request.CompletedByUserId = User.GetUserId(); request.Result = input.Result.Trim(); request.WorkNotes = input.WorkNotes.Trim(); request.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return Ok(await Dto(id, ct)); }

    [HttpPut("{id}/cancel"), Authorize(Roles = "Admin,IT")]
    public async Task<ActionResult<MaintenanceRequestDto>> Cancel(string id, MaintenanceRequestCancelDto input, CancellationToken ct)
    { var request = await db.MaintenanceRequests.FirstOrDefaultAsync(x => x.Id == id, ct); if (request is null) return NotFound(); if (Terminal(request.Status)) return ConflictProblem("Talep zaten kapalı."); request.Status = MaintenanceRequestStatus.Cancelled; request.CancellationReason = input.CancellationReason.Trim(); request.UpdatedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return Ok(await Dto(id, ct)); }

    private IQueryable<MaintenanceRequest> Query() => db.MaintenanceRequests.AsNoTracking();
    private Task<MaintenanceRequestDto> Dto(string id, CancellationToken ct) => Query().Where(x => x.Id == id).Select(ToDto()).FirstAsync(ct);
    private static System.Linq.Expressions.Expression<Func<MaintenanceRequest, MaintenanceRequestDto>> ToDto() => x => new(
        x.Id, "BT-" + x.Id.Substring(0, 8).ToUpper(), x.AssetId, x.Asset.AssetCode, x.Asset.Brand + " " + x.Asset.Model,
        x.RequestedByEmployeeId, x.RequestedByEmployee.FullName, x.RequestedByEmployee.Department, x.Title, x.Description,
        Priority(x.Priority), Status(x.Status), x.AssignedToUserId,
        x.AssignedToUser == null ? null : x.AssignedToUser.Employee != null ? x.AssignedToUser.Employee.FullName : x.AssignedToUser.Username,
        x.CreatedAt, x.UpdatedAt, x.CompletedAt, x.CompletedByUserId,
        x.CompletedByUser == null ? null : x.CompletedByUser.Employee != null ? x.CompletedByUser.Employee.FullName : x.CompletedByUser.Username,
        x.Result, x.WorkNotes, x.CancellationReason);
    private BadRequestObjectResult Bad(string detail) => BadRequest(new ProblemDetails { Status = 400, Title = "Geçersiz istek", Detail = detail });
    private ConflictObjectResult ConflictProblem(string detail) => Conflict(new ProblemDetails { Status = 409, Title = "Destek talebi durumu uygun değil", Detail = detail });
    private static bool Terminal(MaintenanceRequestStatus value) => value is MaintenanceRequestStatus.Completed or MaintenanceRequestStatus.Cancelled;
    private static bool TryPriority(string value, out MaintenanceRequestPriority result) { result = value.Trim() switch { "Düşük" => MaintenanceRequestPriority.Low, "Normal" => MaintenanceRequestPriority.Normal, "Yüksek" => MaintenanceRequestPriority.High, "Kritik" => MaintenanceRequestPriority.Critical, _ => (MaintenanceRequestPriority)(-1) }; return Enum.IsDefined(result); }
    private static bool TryStatus(string value, out MaintenanceRequestStatus result) { result = value.Trim() switch { "Açık" => MaintenanceRequestStatus.Open, "Atandı" => MaintenanceRequestStatus.Assigned, "İşlemde" => MaintenanceRequestStatus.InProgress, "Tamamlandı" => MaintenanceRequestStatus.Completed, "İptal Edildi" => MaintenanceRequestStatus.Cancelled, _ => (MaintenanceRequestStatus)(-1) }; return Enum.IsDefined(result); }
    private static string Priority(MaintenanceRequestPriority value) => value switch { MaintenanceRequestPriority.Low => "Düşük", MaintenanceRequestPriority.High => "Yüksek", MaintenanceRequestPriority.Critical => "Kritik", _ => "Normal" };
    private static string Status(MaintenanceRequestStatus value) => value switch { MaintenanceRequestStatus.Assigned => "Atandı", MaintenanceRequestStatus.InProgress => "İşlemde", MaintenanceRequestStatus.Completed => "Tamamlandı", MaintenanceRequestStatus.Cancelled => "İptal Edildi", _ => "Açık" };
}
