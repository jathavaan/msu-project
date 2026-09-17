using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Transactions.GetTransactions;

public interface IGetTransactionsRepository
{
    Task<List<TransactionVm>> GetTransactions(
        Guid? categoryId, bool uncategorizedOnly, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
}

public sealed class GetTransactionsRepository(FinanceOneDbContext context) : IGetTransactionsRepository
{
    public Task<List<TransactionVm>> GetTransactions(
        Guid? categoryId, bool uncategorizedOnly, DateOnly? from, DateOnly? to, CancellationToken cancellationToken) =>
        context.Transactions
            .Where(t => categoryId == null || t.CategoryId == categoryId)
            .Where(t => !uncategorizedOnly || t.CategoryId == null)
            .Where(t => from == null || t.Date >= from)
            .Where(t => to == null || t.Date <= to)
            .OrderByDescending(t => t.Date)
            .Select(t => new TransactionVm(t.Id, t.Date, t.Description, t.Amount, t.CategoryId, t.Category != null ? t.Category.Name : null))
            .ToListAsync(cancellationToken);
}
