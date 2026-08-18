using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,IT")]
[Route("api/licenses")]
public sealed class LicensesController(
    ApplicationDbContext dbContext,
    LicenseAssignmentService assignmentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LicenseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LicenseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var licenses = await dbContext.Licenses
            .AsNoTracking()
            .OrderBy(license => license.LicenseCode)
            .Select(license => new
            {
                License = license,
                UsedSeats = license.Assignments.Count(assignment => assignment.RevokedAt == null)
            })
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.Today);
        return Ok(licenses.Select(item => ToDto(item.License, item.UsedSeats, today)).ToList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType<LicenseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LicenseDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var license = await dbContext.Licenses
            .AsNoTracking()
            .Where(current => current.Id == id)
            .Select(current => new
            {
                License = current,
                UsedSeats = current.Assignments.Count(assignment => assignment.RevokedAt == null)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return license is null
            ? NotFound()
            : Ok(ToDto(license.License, license.UsedSeats, DateOnly.FromDateTime(DateTime.Today)));
    }

    [HttpPost]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    [ProducesResponseType<LicenseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LicenseDto>> Create(
        LicenseCreateDto request,
        CancellationToken cancellationToken)
    {
        var licenseCode = request.LicenseCode.Trim();

        if (await dbContext.Licenses.AnyAsync(
                license => license.LicenseCode == licenseCode,
                cancellationToken))
        {
            return DuplicateLicenseCodeConflict();
        }

        var license = new License
        {
            Id = Guid.NewGuid().ToString("N"),
            LicenseCode = licenseCode,
            ProductName = request.ProductName.Trim(),
            Vendor = request.Vendor.Trim(),
            LicenseType = request.LicenseType.Trim(),
            TotalSeats = request.TotalSeats,
            LegacyUsedSeats = 0,
            StartDate = request.StartDate!.Value,
            ExpirationDate = request.ExpirationDate,
            IsActive = request.IsActive,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        dbContext.Licenses.Add(license);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return DuplicateLicenseCodeConflict();
        }

        var response = ToDto(license, 0, DateOnly.FromDateTime(DateTime.Today));
        return CreatedAtAction(nameof(GetById), new { id = license.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    [ProducesResponseType<LicenseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LicenseDto>> Update(
        string id,
        LicenseUpdateDto request,
        CancellationToken cancellationToken)
    {
        var license = await dbContext.Licenses
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (license is null)
        {
            return NotFound();
        }

        var licenseCode = request.LicenseCode.Trim();

        var activeAssignmentCount = await dbContext.LicenseAssignments.CountAsync(
            assignment => assignment.LicenseId == id && assignment.RevokedAt == null,
            cancellationToken);
        if (request.TotalSeats < activeAssignmentCount)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Toplam lisans hakkı azaltılamadı",
                Detail = "Toplam lisans hakkı aktif atama sayısından düşük olamaz."
            });
        }

        if (await dbContext.Licenses.AnyAsync(
                current => current.Id != id && current.LicenseCode == licenseCode,
                cancellationToken))
        {
            return DuplicateLicenseCodeConflict();
        }

        license.LicenseCode = licenseCode;
        license.ProductName = request.ProductName.Trim();
        license.Vendor = request.Vendor.Trim();
        license.LicenseType = request.LicenseType.Trim();
        license.TotalSeats = request.TotalSeats;
        license.StartDate = request.StartDate!.Value;
        license.ExpirationDate = request.ExpirationDate;
        license.IsActive = request.IsActive;
        license.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return DuplicateLicenseCodeConflict();
        }

        return Ok(ToDto(license, activeAssignmentCount, DateOnly.FromDateTime(DateTime.Today)));
    }

    [HttpGet("{id}/assignments")]
    public async Task<ActionResult<IReadOnlyList<LicenseAssignmentDto>>> GetAssignments(
        string id,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Licenses.AsNoTracking().AnyAsync(item => item.Id == id, cancellationToken))
            return NotFound();
        return Ok(await assignmentService.GetByLicenseAsync(id, cancellationToken));
    }

    [HttpGet("assignments/asset/{assetId}")]
    public async Task<ActionResult<IReadOnlyList<LicenseAssignmentDto>>> GetAssetAssignments(
        string assetId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Assets.AsNoTracking().AnyAsync(item => item.Id == assetId, cancellationToken))
            return NotFound();
        return Ok(await assignmentService.GetActiveByAssetAsync(assetId, cancellationToken));
    }

    [HttpPost("{id}/assignments")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<LicenseAssignmentDto>> CreateAssignment(
        string id,
        LicenseAssignmentCreateDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        var result = await assignmentService.CreateAsync(id, request, userId, cancellationToken);
        return result.Status switch
        {
            LicenseAssignmentOperationStatus.Success => CreatedAtAction(
                nameof(GetAssignments),
                new { id },
                result.Assignment),
            LicenseAssignmentOperationStatus.NotFound => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Lisans bulunamadı",
                Detail = result.ErrorMessage
            }),
            LicenseAssignmentOperationStatus.Conflict => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Lisans atanamadı",
                Detail = result.ErrorMessage
            }),
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Lisans atanamadı",
                Detail = result.ErrorMessage
            })
        };
    }

    [HttpPut("{licenseId}/assignments/{assignmentId}/revoke")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    public async Task<ActionResult<LicenseAssignmentDto>> RevokeAssignment(
        string licenseId,
        string assignmentId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        var result = await assignmentService.RevokeAsync(
            licenseId,
            assignmentId,
            userId,
            cancellationToken);
        return result.Status switch
        {
            LicenseAssignmentOperationStatus.Success => Ok(result.Assignment),
            LicenseAssignmentOperationStatus.NotFound => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Lisans ataması bulunamadı",
                Detail = result.ErrorMessage
            }),
            _ => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Lisans ataması kaldırılamadı",
                Detail = result.ErrorMessage
            })
        };
    }

    private ConflictObjectResult DuplicateLicenseCodeConflict() =>
        Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Tekrarlanan lisans kodu",
            Detail = "Bu lisans kodu başka bir kayıtta kullanılıyor."
        });

    private static LicenseDto ToDto(License license, int usedSeats, DateOnly today)
    {
        var status = !license.IsActive
            ? "Pasif"
            : license.ExpirationDate switch
            {
                { } expirationDate when expirationDate < today => "Süresi Doldu",
                { } expirationDate when expirationDate.DayNumber - today.DayNumber <= 30 => "Yaklaşıyor",
                _ => "Aktif"
            };

        return new LicenseDto(
            license.Id,
            license.LicenseCode,
            license.ProductName,
            license.Vendor,
            license.LicenseType,
            license.TotalSeats,
            usedSeats,
            license.TotalSeats - usedSeats,
            license.StartDate,
            license.ExpirationDate,
            license.IsActive,
            license.Notes,
            status);
    }
}
