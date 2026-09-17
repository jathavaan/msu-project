using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.Transactions.ImportTransactions;

public interface IImportTransactionsRepository
{
    Task<List<CategorizationRule>> GetCategorizationRules(CancellationToken cancellationToken);

    /// <summary>Which of the given hashes already exist in the Transactions table.</summary>
    Task<HashSet<string>> GetExistingHashes(List<string> hashes, CancellationToken cancellationToken);

    Task AddRange(List<Transaction> transactions, CancellationToken cancellationToken);
}

public sealed class ImportTransactionsRepository(FinanceOneDbContext context) : IImportTransactionsRepository
{
    public Task<List<CategorizationRule>> GetCategorizationRules(CancellationToken cancellationToken) =>
        context.CategorizationRules.ToListAsync(cancellationToken);

    public async Task<HashSet<string>> GetExistingHashes(List<string> hashes, CancellationToken cancellationToken)
    {
        var existing = await context.Transactions
            .Where(t => hashes.Contains(t.Hash))
            .Select(t => t.Hash)
            .ToListAsync(cancellationToken);
        return existing.ToHashSet();
    }

    public async Task AddRange(List<Transaction> transactions, CancellationToken cancellationToken)
    {
        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync(cancellationToken);
    }
}
