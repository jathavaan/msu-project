using Microsoft.EntityFrameworkCore;

namespace FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

public interface IGetBalanceForecastRepository
{
    Task<List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>> GetRecurringIncomes(CancellationToken cancellationToken);
    Task<List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>> GetRecurringExpenses(CancellationToken cancellationToken);
    Task<List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>> GetRecurringMonthlySavings(CancellationToken cancellationToken);
    Task<int?> GetPeriodStartDay(CancellationToken cancellationToken);
}

// This slice doesn't own a table, so it queries Income, Expenses, MonthlySavings and AppSettings
// directly via FinanceOneDbContext rather than through those slices' own repositories (see
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

    // CategoryName here is the linked SavingGoal's name, not a Category — a MonthlySaving links to
    // a SavingGoal, not a Category (see Domain/Entites/MonthlySaving.cs), but the tuple shape stays
    // identical to income/expenses so the handler can walk all three the same way.
    public Task<List<(string Name, string CategoryName, decimal Amount, int RecurrenceDay)>> GetRecurringMonthlySavings(CancellationToken cancellationToken) =>
        context.MonthlySavings
            .Select(m => new ValueTuple<string, string, decimal, int>(m.Name, m.SavingGoal!.Name, m.Amount, m.RecurrenceDay))
            .ToListAsync(cancellationToken);

    public Task<int?> GetPeriodStartDay(CancellationToken cancellationToken) =>
        context.AppSettings.Select(s => (int?)s.PeriodStartDay).FirstOrDefaultAsync(cancellationToken);
}
