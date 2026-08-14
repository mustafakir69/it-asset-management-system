using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;
using TakipProgrami.Api.Helpers;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/licenses")]
public sealed class LicensesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LicenseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LicenseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var licenses = await dbContext.Licenses
            .AsNoTracking()
            .OrderBy(license => license.LicenseCode)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.Today);
        return Ok(licenses.Select(license => ToDto(license, today)).ToList());
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
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        return license is null
            ? NotFound()
            : Ok(ToDto(license, DateOnly.FromDateTime(DateTime.Today)));
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
            UsedSeats = request.UsedSeats,
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

        var response = ToDto(license, DateOnly.FromDateTime(DateTime.Today));
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
        license.UsedSeats = request.UsedSeats;
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

        return Ok(ToDto(license, DateOnly.FromDateTime(DateTime.Today)));
    }

    private ConflictObjectResult DuplicateLicenseCodeConflict() =>
        Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Tekrarlanan lisans kodu",
            Detail = "Bu lisans kodu başka bir kayıtta kullanılıyor."
        });

    private static LicenseDto ToDto(License license, DateOnly today)
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
            license.UsedSeats,
            license.TotalSeats - license.UsedSeats,
            license.StartDate,
            license.ExpirationDate,
            license.IsActive,
            license.Notes,
            status);
    }
}
