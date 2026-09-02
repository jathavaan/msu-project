using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.UpcomingPayments.GetUpcomingPayments;

public interface IGetUpcomingPaymentsRepository
{
    Task<List<(string Name, decimal Amount, int RecurrenceDay)>> GetRecurringIncomes(CancellationToken cancellationToken);
    Task<List<(string Name, decimal Amount, int RecurrenceDay)>> GetRecurringExpenses(CancellationToken cancellationToken);
}

// This slice doesn't own a table, so it queries Income and Expenses directly via
// FinanceOneDbContext rather than through those slices' own repositories (see
// server/FinanceOne/CLAUDE.md > Repositories).
public sealed class GetUpcomingPaymentsRepository(FinanceOneDbContext context) : IGetUpcomingPaymentsRepository
{
    public Task<List<(string Name, decimal Amount, int RecurrenceDay)>> GetRecurringIncomes(CancellationToken cancellationToken) =>
        context.Incomes
            .Select(i => new ValueTuple<string, decimal, int>(i.Name, i.Amount, i.RecurrenceDay))
            .ToListAsync(cancellationToken);

    public Task<List<(string Name, decimal Amount, int RecurrenceDay)>> GetRecurringExpenses(CancellationToken cancellationToken) =>
        context.Expenses
            .Select(e => new ValueTuple<string, decimal, int>(e.Name, e.Amount, e.RecurrenceDay))
            .ToListAsync(cancellationToken);
}
