using System.Data;
using Microsoft.EntityFrameworkCore;
using TakipProgrami.Api.Data;
using TakipProgrami.Api.DTOs;
using TakipProgrami.Api.Entities;

namespace TakipProgrami.Api.Services;

public enum StockTransactionResultStatus
{
    Success,
    StockItemNotFound,
    InvalidTransactionType,
    InvalidQuantity,
    InsufficientStock
}

public sealed record StockTransactionResult(
    StockTransactionResultStatus Status,
    StockTransactionDto? Transaction = null,
    string? ErrorMessage = null);

public sealed class StockService(
    ApplicationDbContext dbContext,
    NotificationService notificationService)
{
    public async Task<StockTransactionResult> CreateTransactionAsync(
        string stockItemId,
        StockTransactionCreateDto request,
        string currentUserId,
        CancellationToken cancellationToken)
    {
        if (!TryParseTransactionType(request.TransactionType, out var transactionType))
        {
            return new(
                StockTransactionResultStatus.InvalidTransactionType,
                ErrorMessage: "İşlem tipi yalnızca Giriş veya Çıkış olabilir.");
        }

        if (request.Quantity <= 0)
        {
            return new(
                StockTransactionResultStatus.InvalidQuantity,
                ErrorMessage: "Miktar sıfırdan büyük olmalıdır.");
        }

        await using var databaseTransaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var stockItem = await dbContext.StockItems
            .FirstOrDefaultAsync(item => item.Id == stockItemId, cancellationToken);

        if (stockItem is null)
        {
            return new(
                StockTransactionResultStatus.StockItemNotFound,
                ErrorMessage: "Stok ürünü bulunamadı.");
        }

        if (transactionType == StockTransactionType.Exit &&
            stockItem.CurrentQuantity < request.Quantity)
        {
            return new(
                StockTransactionResultStatus.InsufficientStock,
                ErrorMessage: $"Stok çıkışı yapılamadı. Mevcut stok {stockItem.CurrentQuantity} {stockItem.Unit}.");
        }

        Employee? recipient = null;
        if (!string.IsNullOrWhiteSpace(request.RecipientEmployeeId))
        {
            if (transactionType == StockTransactionType.Entry)
                return new(StockTransactionResultStatus.InvalidTransactionType, ErrorMessage: "Stok girişinde teslim alan çalışan seçilemez.");
            recipient = await dbContext.Employees.FirstOrDefaultAsync(x => x.Id == request.RecipientEmployeeId && x.IsActive, cancellationToken);
            if (recipient is null)
                return new(StockTransactionResultStatus.InvalidTransactionType, ErrorMessage: "Teslim alan aktif çalışan bulunamadı.");
        }

        stockItem.CurrentQuantity += transactionType == StockTransactionType.Entry
            ? request.Quantity
            : -request.Quantity;

        var transaction = new StockTransaction
        {
            Id = Guid.NewGuid().ToString("N"),
            StockItemId = stockItem.Id,
            TransactionType = transactionType,
            Quantity = request.Quantity,
            TransactionDate = request.TransactionDate!.Value,
            PerformedByUserId = currentUserId,
            RecipientEmployeeId = recipient?.Id,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim()
        };

        dbContext.StockTransactions.Add(transaction);
        var stockAlert = await notificationService.SyncStockAlertAsync(
            stockItem,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await databaseTransaction.CommitAsync(cancellationToken);

        if (stockAlert is not null)
        {
            await notificationService.DeliverStockAlertAsync(
                stockAlert,
                stockItem,
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        transaction.PerformedByUser = await dbContext.AppUsers.Include(x => x.Employee).FirstAsync(x => x.Id == currentUserId, cancellationToken);
        transaction.RecipientEmployee = recipient;
        return new(StockTransactionResultStatus.Success, ToDto(transaction));
    }

    public static StockTransactionDto ToDto(StockTransaction transaction) =>
        new(
            transaction.Id,
            transaction.StockItemId,
            transaction.TransactionType == StockTransactionType.Entry ? "Giriş" : "Çıkış",
            transaction.Quantity,
            transaction.TransactionDate,
            transaction.PerformedByUserId,
            transaction.PerformedByUser.Employee?.FullName ?? transaction.PerformedByUser.Username,
            transaction.RecipientEmployeeId,
            transaction.RecipientEmployee?.FullName,
            transaction.Note);

    private static bool TryParseTransactionType(
        string value,
        out StockTransactionType transactionType)
    {
        var normalizedValue = value.Trim();

        if (string.Equals(normalizedValue, "Giriş", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalizedValue, "Giris", StringComparison.OrdinalIgnoreCase))
        {
            transactionType = StockTransactionType.Entry;
            return true;
        }

        if (string.Equals(normalizedValue, "Çıkış", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalizedValue, "Cikis", StringComparison.OrdinalIgnoreCase))
        {
            transactionType = StockTransactionType.Exit;
            return true;
        }

        transactionType = default;
        return false;
    }
}
