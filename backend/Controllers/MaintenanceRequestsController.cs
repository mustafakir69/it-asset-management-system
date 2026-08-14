using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/maintenance/requests")]
public sealed class MaintenanceRequestsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MaintenanceRequestDto>>> GetAll(
        string? status,
        string? priority,
        string? search,
        CancellationToken cancellationToken)
    {
        var requests = await dbContext.MaintenanceRequests.AsNoTracking().Include(item => item.Asset)
            .OrderByDescending(item => item.CreatedAt).ToListAsync(cancellationToken);

        var filtered = requests.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!TryParseStatus(status, out var parsedStatus)) return BadRequestProblem("Geçersiz bakım talebi durumu.");
            filtered = filtered.Where(item => item.Status == parsedStatus);
        }
        if (!string.IsNullOrWhiteSpace(priority))
        {
            if (!TryParsePriority(priority, out var parsedPriority)) return BadRequestProblem("Geçersiz bakım talebi önceliği.");
            filtered = filtered.Where(item => item.Priority == parsedPriority);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            filtered = filtered.Where(item =>
                item.Asset.AssetCode.Contains(normalized, StringComparison.CurrentCultureIgnoreCase) ||
                $"{item.Asset.Brand} {item.Asset.Model}".Contains(normalized, StringComparison.CurrentCultureIgnoreCase) ||
                item.Title.Contains(normalized, StringComparison.CurrentCultureIgnoreCase) ||
                item.RequestedBy.Contains(normalized, StringComparison.CurrentCultureIgnoreCase) ||
                (item.AssignedTechnician?.Contains(normalized, StringComparison.CurrentCultureIgnoreCase) ?? false));
        }
        return Ok(filtered.Select(ToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenanceRequestDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var request = await dbContext.MaintenanceRequests.AsNoTracking().Include(item => item.Asset)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return request is null ? NotFound() : Ok(ToDto(request));
    }

    [HttpPost]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<MaintenanceRequestDto>> Create(
        MaintenanceRequestCreateDto input,
        CancellationToken cancellationToken)
    {
        if (!TryParsePriority(input.Priority, out var priority)) return BadRequestProblem("Geçersiz bakım talebi önceliği.");
        var asset = await GetEligibleAsset(input.AssetId, cancellationToken);
        if (asset is null) return ValidationProblem(ModelState);
        var now = DateTimeOffset.UtcNow;
        var request = new MaintenanceRequest
        {
            Id = Guid.NewGuid().ToString("N"), AssetId = asset.Id, Asset = asset,
            Title = input.Title.Trim(), Description = input.Description.Trim(), Priority = priority,
            Status = MaintenanceRequestStatus.Open, RequestedBy = input.RequestedBy.Trim(),
            CreatedAt = now, UpdatedAt = now
        };
        dbContext.MaintenanceRequests.Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, ToDto(request));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<MaintenanceRequestDto>> Update(
        string id,
        MaintenanceRequestUpdateDto input,
        CancellationToken cancellationToken)
    {
        var request = await dbContext.MaintenanceRequests.Include(item => item.Asset)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (request is null) return NotFound();
        if (IsTerminal(request.Status)) return StateConflict("Tamamlanmış veya iptal edilmiş talep düzenlenemez.");
        if (!TryParsePriority(input.Priority, out var priority)) return BadRequestProblem("Geçersiz bakım talebi önceliği.");
        var asset = await GetEligibleAsset(input.AssetId, cancellationToken);
        if (asset is null) return ValidationProblem(ModelState);
        request.AssetId = asset.Id;
        request.Asset = asset;
        request.Title = input.Title.Trim();
        request.Description = input.Description.Trim();
        request.Priority = priority;
        request.RequestedBy = input.RequestedBy.Trim();
        request.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(request));
    }

    [HttpPut("{id}/assign")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<MaintenanceRequestDto>> Assign(
        string id,
        MaintenanceRequestAssignDto input,
        CancellationToken cancellationToken)
    {
        var request = await FindTracked(id, cancellationToken);
        if (request is null) return NotFound();
        if (IsTerminal(request.Status)) return StateConflict("Tamamlanmış veya iptal edilmiş talep atanamaz.");
        request.AssignedTechnician = input.AssignedTechnician.Trim();
        request.Status = MaintenanceRequestStatus.Assigned;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(request));
    }

    [HttpPut("{id}/start")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<MaintenanceRequestDto>> Start(string id, CancellationToken cancellationToken)
    {
        var request = await FindTracked(id, cancellationToken);
        if (request is null) return NotFound();
        if (request.Status != MaintenanceRequestStatus.Assigned)
            return StateConflict("Yalnızca teknisyene atanmış talepler işleme alınabilir.");
        request.Status = MaintenanceRequestStatus.InProgress;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(request));
    }

    [HttpPut("{id}/complete")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<MaintenanceRequestDto>> Complete(
        string id,
        MaintenanceRequestCompleteDto input,
        CancellationToken cancellationToken)
    {
        var request = await FindTracked(id, cancellationToken);
        if (request is null) return NotFound();
        if (IsTerminal(request.Status)) return StateConflict(request.Status == MaintenanceRequestStatus.Completed ? "Bu talep zaten tamamlanmış." : "İptal edilmiş talep tamamlanamaz.");
        request.Status = MaintenanceRequestStatus.Completed;
        request.CompletedAt = input.CompletedAt!.Value;
        request.CompletedBy = input.CompletedBy.Trim();
        request.Result = input.Result.Trim();
        request.WorkNotes = input.WorkNotes.Trim();
        request.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(request));
    }

    [HttpPut("{id}/cancel")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<MaintenanceRequestDto>> Cancel(
        string id,
        MaintenanceRequestCancelDto input,
        CancellationToken cancellationToken)
    {
        var request = await FindTracked(id, cancellationToken);
        if (request is null) return NotFound();
        if (IsTerminal(request.Status)) return StateConflict(request.Status == MaintenanceRequestStatus.Completed ? "Tamamlanmış talep iptal edilemez." : "Bu talep zaten iptal edilmiş.");
        request.Status = MaintenanceRequestStatus.Cancelled;
        request.CancellationReason = input.CancellationReason.Trim();
        request.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(request));
    }

    private Task<MaintenanceRequest?> FindTracked(string id, CancellationToken cancellationToken) =>
        dbContext.MaintenanceRequests.Include(item => item.Asset)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    private async Task<Asset?> GetEligibleAsset(string assetId, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Assets.FirstOrDefaultAsync(item => item.Id == assetId, cancellationToken);
        if (asset is null)
        {
            ModelState.AddModelError(nameof(MaintenanceRequestCreateDto.AssetId), "Seçilen cihaz bulunamadı.");
            return null;
        }
        if (asset.Status is "Hurda" or "Elden çıkarıldı")
        {
            ModelState.AddModelError(nameof(MaintenanceRequestCreateDto.AssetId), "Hurda veya elden çıkarılmış cihaz için bakım talebi oluşturulamaz.");
            return null;
        }
        return asset;
    }

    private BadRequestObjectResult BadRequestProblem(string detail) => BadRequest(new ProblemDetails
    {
        Status = StatusCodes.Status400BadRequest, Title = "Geçersiz istek", Detail = detail
    });

    private ConflictObjectResult StateConflict(string detail) => Conflict(new ProblemDetails
    {
        Status = StatusCodes.Status409Conflict, Title = "Bakım talebi durumu uygun değil", Detail = detail
    });

    private static bool IsTerminal(MaintenanceRequestStatus status) =>
        status is MaintenanceRequestStatus.Completed or MaintenanceRequestStatus.Cancelled;

    private static bool TryParsePriority(string value, out MaintenanceRequestPriority priority)
    {
        priority = value.Trim() switch
        {
            "Düşük" => MaintenanceRequestPriority.Low,
            "Normal" => MaintenanceRequestPriority.Normal,
            "Yüksek" => MaintenanceRequestPriority.High,
            "Kritik" => MaintenanceRequestPriority.Critical,
            _ => (MaintenanceRequestPriority)(-1)
        };
        return Enum.IsDefined(priority);
    }

    private static bool TryParseStatus(string value, out MaintenanceRequestStatus status)
    {
        status = value.Trim() switch
        {
            "Açık" => MaintenanceRequestStatus.Open,
            "Atandı" => MaintenanceRequestStatus.Assigned,
            "İşlemde" => MaintenanceRequestStatus.InProgress,
            "Tamamlandı" => MaintenanceRequestStatus.Completed,
            "İptal Edildi" => MaintenanceRequestStatus.Cancelled,
            _ => (MaintenanceRequestStatus)(-1)
        };
        return Enum.IsDefined(status);
    }

    private static MaintenanceRequestDto ToDto(MaintenanceRequest request) => new(
        request.Id, $"BT-{request.Id[..Math.Min(8, request.Id.Length)].ToUpperInvariant()}",
        request.AssetId, request.Asset.AssetCode, $"{request.Asset.Brand} {request.Asset.Model}",
        request.Title, request.Description, PriorityLabel(request.Priority), StatusLabel(request.Status),
        request.RequestedBy, request.AssignedTechnician, request.CreatedAt, request.UpdatedAt,
        request.CompletedAt, request.CompletedBy, request.Result, request.WorkNotes, request.CancellationReason);

    private static string PriorityLabel(MaintenanceRequestPriority priority) => priority switch
    {
        MaintenanceRequestPriority.Low => "Düşük",
        MaintenanceRequestPriority.High => "Yüksek",
        MaintenanceRequestPriority.Critical => "Kritik",
        _ => "Normal"
    };

    private static string StatusLabel(MaintenanceRequestStatus status) => status switch
    {
        MaintenanceRequestStatus.Assigned => "Atandı",
        MaintenanceRequestStatus.InProgress => "İşlemde",
        MaintenanceRequestStatus.Completed => "Tamamlandı",
        MaintenanceRequestStatus.Cancelled => "İptal Edildi",
        _ => "Açık"
    };
}
