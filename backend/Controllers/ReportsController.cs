using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Helpers;
using TakipProgrami.Api.Services;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,IT")]
[Route("api/reports")]
public sealed class ReportsController(ReportsService reportsService) : ControllerBase
{
    [HttpGet("inventory")]
    public async Task<ActionResult<IReadOnlyList<InventoryReportDto>>> Inventory(
        string? category, string? status, string? location, CancellationToken cancellationToken) =>
        Ok(await reportsService.GetInventoryAsync(category, status, location, cancellationToken));

    [HttpGet("inventory/csv")]
    public async Task<IActionResult> InventoryCsv(
        string? category, string? status, string? location, CancellationToken cancellationToken) =>
        Csv(CsvExporter.Inventory(await reportsService.GetInventoryAsync(category, status, location, cancellationToken)), "envanter-raporu.csv");

    [HttpGet("assignments")]
    public async Task<ActionResult<IReadOnlyList<AssignmentReportDto>>> Assignments(
        string? status, string? department, DateTimeOffset? from, DateTimeOffset? to,
        CancellationToken cancellationToken) =>
        Ok(await reportsService.GetAssignmentsAsync(status, department, from, to, cancellationToken));

    [HttpGet("assignments/csv")]
    public async Task<IActionResult> AssignmentsCsv(
        string? status, string? department, DateTimeOffset? from, DateTimeOffset? to,
        CancellationToken cancellationToken) =>
        Csv(CsvExporter.Assignments(await reportsService.GetAssignmentsAsync(status, department, from, to, cancellationToken)), "zimmet-raporu.csv");

    [HttpGet("stock")]
    public async Task<ActionResult<IReadOnlyList<StockReportDto>>> Stock(
        string? category, string? location, bool? critical, CancellationToken cancellationToken) =>
        Ok(await reportsService.GetStockAsync(category, location, critical, cancellationToken));

    [HttpGet("stock/csv")]
    public async Task<IActionResult> StockCsv(
        string? category, string? location, bool? critical, CancellationToken cancellationToken) =>
        Csv(CsvExporter.Stock(await reportsService.GetStockAsync(category, location, critical, cancellationToken)), "stok-raporu.csv");

    [HttpGet("stock-transactions")]
    public async Task<ActionResult<IReadOnlyList<StockMovementReportDto>>> StockTransactions(
        CancellationToken cancellationToken) =>
        Ok(await reportsService.GetStockMovementsAsync(cancellationToken));

    [HttpGet("maintenance")]
    public async Task<ActionResult<MaintenanceReportResponseDto>> Maintenance(
        string? recordType, string? status, CancellationToken cancellationToken) =>
        Ok(await reportsService.GetMaintenanceAsync(recordType, status, cancellationToken));

    [HttpGet("maintenance/csv")]
    public async Task<IActionResult> MaintenanceCsv(
        string? recordType, string? status, CancellationToken cancellationToken)
    {
        var report = await reportsService.GetMaintenanceAsync(recordType, status, cancellationToken);
        return Csv(CsvExporter.Maintenance(report.Records), "bakim-raporu.csv");
    }

    [HttpGet("warranties")]
    public async Task<ActionResult<IReadOnlyList<WarrantyReportDto>>> Warranties(
        string? warrantyStatus,
        string? assetStatus,
        CancellationToken cancellationToken) =>
        Ok(await reportsService.GetWarrantiesAsync(warrantyStatus, assetStatus, cancellationToken));

    [HttpGet("warranties/csv")]
    public async Task<IActionResult> WarrantiesCsv(
        string? warrantyStatus,
        string? assetStatus,
        CancellationToken cancellationToken) =>
        Csv(
            CsvExporter.Warranties(await reportsService.GetWarrantiesAsync(
                warrantyStatus,
                assetStatus,
                cancellationToken)),
            "garanti-raporu.csv");

    [HttpGet("licenses")]
    public async Task<ActionResult<IReadOnlyList<LicenseReportDto>>> Licenses(
        string? status,
        CancellationToken cancellationToken) =>
        Ok(await reportsService.GetLicensesAsync(status, cancellationToken));

    [HttpGet("licenses/csv")]
    public async Task<IActionResult> LicensesCsv(
        string? status,
        CancellationToken cancellationToken) =>
        Csv(
            CsvExporter.Licenses(await reportsService.GetLicensesAsync(status, cancellationToken)),
            "lisans-raporu.csv");

    [HttpGet("support-requests")]
    public async Task<ActionResult<IReadOnlyList<SupportReportDto>>> SupportRequests(
        string? status,
        string? priority,
        CancellationToken cancellationToken) =>
        Ok(await reportsService.GetSupportRequestsAsync(status, priority, cancellationToken));

    [HttpGet("support-requests/csv")]
    public async Task<IActionResult> SupportRequestsCsv(
        string? status,
        string? priority,
        CancellationToken cancellationToken) =>
        Csv(
            CsvExporter.SupportRequests(await reportsService.GetSupportRequestsAsync(
                status,
                priority,
                cancellationToken)),
            "teknik-destek-raporu.csv");

    private FileContentResult Csv(byte[] content, string fileName) =>
        File(content, "text/csv; charset=utf-8", fileName);
}
