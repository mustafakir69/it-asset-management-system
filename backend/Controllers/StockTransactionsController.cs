using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Controllers;

[ApiController]
[Route("api/stock-transactions")]
public sealed class StockTransactionsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<StockTransactionListDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockTransactionListDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var transactions = await dbContext.StockTransactions
            .AsNoTracking()
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .Select(transaction => new StockTransactionListDto(
                transaction.Id,
                transaction.StockItemId,
                transaction.StockItem.ItemCode,
                transaction.StockItem.Name,
                transaction.TransactionType == StockTransactionType.Entry ? "Giriş" : "Çıkış",
                transaction.Quantity,
                transaction.TransactionDate,
                transaction.PersonName,
                transaction.Note))
            .ToListAsync(cancellationToken);

        return Ok(transactions);
    }
}
