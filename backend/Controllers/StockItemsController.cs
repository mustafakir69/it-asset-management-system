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
[Route("api/stock-items")]
public sealed class StockItemsController(
    ApplicationDbContext dbContext,
    StockService stockService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<StockItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockItemDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var stockItems = await dbContext.StockItems
            .AsNoTracking()
            .OrderBy(item => item.ItemCode)
            .Select(item => new StockItemDto(
                item.Id,
                item.ItemCode,
                item.Name,
                item.Category,
                item.BrandModel,
                item.Unit,
                item.CurrentQuantity,
                item.MinimumQuantity,
                item.Location,
                item.IsActive,
                item.CurrentQuantity <= item.MinimumQuantity))
            .ToListAsync(cancellationToken);

        return Ok(stockItems);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<StockItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockItemDto>> GetById(
        string id,
        CancellationToken cancellationToken)
    {
        var stockItem = await dbContext.StockItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return stockItem is null ? NotFound() : Ok(ToDto(stockItem));
    }

    [HttpPost]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    [ProducesResponseType<StockItemDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StockItemDto>> Create(
        StockItemCreateDto request,
        CancellationToken cancellationToken)
    {
        var itemCode = request.ItemCode.Trim();

        if (await dbContext.StockItems.AnyAsync(
                item => item.ItemCode == itemCode,
                cancellationToken))
        {
            return DuplicateItemCodeConflict();
        }

        var stockItem = new StockItem
        {
            Id = Guid.NewGuid().ToString("N"),
            ItemCode = itemCode,
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            BrandModel = request.BrandModel.Trim(),
            Unit = request.Unit.Trim(),
            CurrentQuantity = request.CurrentQuantity,
            MinimumQuantity = request.MinimumQuantity,
            Location = request.Location.Trim(),
            IsActive = true
        };

        dbContext.StockItems.Add(stockItem);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return DuplicateItemCodeConflict();
        }

        var response = ToDto(stockItem);
        return CreatedAtAction(nameof(GetById), new { id = stockItem.Id }, response);
    }

    [HttpPut("{id}/minimum-quantity")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    [ProducesResponseType<StockItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockItemDto>> UpdateMinimumQuantity(
        string id,
        StockItemMinimumQuantityUpdateDto request,
        CancellationToken cancellationToken)
    {
        var result = await stockService.UpdateMinimumQuantityAsync(
            id,
            request.MinimumQuantity,
            cancellationToken);

        return result.Status switch
        {
            StockItemUpdateResultStatus.Success => Ok(result.StockItem),
            StockItemUpdateResultStatus.StockItemNotFound => NotFoundProblem(result.ErrorMessage!),
            _ => BadRequestProblem(result.ErrorMessage!)
        };
    }

    [HttpPost("{id}/transactions")]
    [Authorize(Policy = AppAuthorizationPolicies.ManagementWrite)]
    [ProducesResponseType<StockTransactionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StockTransactionDto>> CreateTransaction(
        string id,
        StockTransactionCreateDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        var result = await stockService.CreateTransactionAsync(id, request, userId, cancellationToken);

        return result.Status switch
        {
            StockTransactionResultStatus.Success => Created(
                $"/api/stock-items/{id}/transactions",
                result.Transaction),
            StockTransactionResultStatus.StockItemNotFound => NotFoundProblem(result.ErrorMessage!),
            StockTransactionResultStatus.InsufficientStock => ConflictProblem(result.ErrorMessage!),
            _ => BadRequestProblem(result.ErrorMessage!)
        };
    }

    [HttpGet("{id}/transactions")]
    [ProducesResponseType<IReadOnlyList<StockTransactionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StockTransactionDto>>> GetTransactions(
        string id,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.StockItems.AnyAsync(item => item.Id == id, cancellationToken))
        {
            return NotFoundProblem("Stok ürünü bulunamadı.");
        }

        var transactions = await dbContext.StockTransactions
            .Include(x => x.PerformedByUser).ThenInclude(x => x.Employee)
            .Include(x => x.RecipientEmployee)
            .AsNoTracking()
            .Where(transaction => transaction.StockItemId == id)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .ToListAsync(cancellationToken);

        return Ok(transactions.Select(StockService.ToDto).ToList());
    }

    private ConflictObjectResult DuplicateItemCodeConflict() =>
        Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Tekrarlanan ürün kodu",
            Detail = "Bu ürün kodu başka bir stok ürününde kullanılıyor."
        });

    private BadRequestObjectResult BadRequestProblem(string detail) =>
        BadRequest(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Geçersiz stok hareketi",
            Detail = detail
        });

    private NotFoundObjectResult NotFoundProblem(string detail) =>
        NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Stok ürünü bulunamadı",
            Detail = detail
        });

    private ConflictObjectResult ConflictProblem(string detail) =>
        Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Yetersiz stok",
            Detail = detail
        });

    private static StockItemDto ToDto(StockItem item) =>
        StockService.ToDto(item);
}
