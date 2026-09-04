using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

public interface IGetBalanceForecastRepository
{
    Task<List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>> GetRecurringIncomes(CancellationToken cancellationToken);
    Task<List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>> GetRecurringExpenses(CancellationToken cancellationToken);
}

// This slice doesn't own a table, so it queries Income and Expenses directly via
// FinanceOneDbContext rather than through those slices' own repositories (see
// server/FinanceOne/CLAUDE.md > Repositories).
public sealed class GetBalanceForecastRepository(FinanceOneDbContext context) : IGetBalanceForecastRepository
{
    public Task<List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>> GetRecurringIncomes(CancellationToken cancellationToken) =>
        context.Incomes
            .Select(i => new ValueTuple<string, string, decimal, int>(i.Name, i.Category!.Name, i.Amount, i.RecurrenceDay))
            .ToListAsync(cancellationToken);

    public Task<List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>> GetRecurringExpenses(CancellationToken cancellationToken) =>
        context.Expenses
            .Select(e => new ValueTuple<string, string, decimal, int>(e.Name, e.Category!.Name, e.Amount, e.RecurrenceDay))
            .ToListAsync(cancellationToken);
}
