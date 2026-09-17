namespace FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

public sealed class GetBalanceForecastHandler(IGetBalanceForecastRepository repository)
    : IRequestHandler<GetBalanceForecastQuery, Response<List<BalanceForecastPointVm>>>
{
    // Recurrence is day-of-month (1-28, see Income.RecurrenceDay/Expense.RecurrenceDay/
    // MonthlySaving.RecurrenceDay), so every month can be walked as this same fixed 28-day period
    // regardless of which month it actually is.
    private const int PeriodDays = 28;

    // No AppSettings row yet means the period start day hasn't been configured — default to a
    // plain calendar month, same default GetSettings/GetSettingsHandler uses.
    private const int DefaultPeriodStartDay = 1;

    public async Task<Response<List<BalanceForecastPointVm>>> Handle(GetBalanceForecastQuery request, CancellationToken cancellationToken)
    {
        var incomes = await repository.GetRecurringIncomes(cancellationToken);
        var expenses = await repository.GetRecurringExpenses(cancellationToken);
        var savings = await repository.GetRecurringMonthlySavings(cancellationToken);
        var periodStartDay = await repository.GetPeriodStartDay(cancellationToken) ?? DefaultPeriodStartDay;

        // Every month walks the same recurring income/expenses/savings, so last month ended
        // exactly this much above where it started — that rolls over as this month's starting
        // balance too, instead of resetting back to 0 on day 1. A monthly saving leaving the
        // account counts against the total the same way an expense does.
        var totalNet = incomes.Sum(i => i.Amount) - expenses.Sum(e => e.Amount) - savings.Sum(s => s.Amount);

        var points = new List<BalanceForecastPointVm>(PeriodDays);
        var balance = totalNet;

        for (var offset = 0; offset < PeriodDays; offset++)
        {
            // Walks the fixed 28-day period starting from periodStartDay and wrapping across the
            // month boundary (e.g. a start day of 25 walks 25, 26, 27, 28, 1, 2, ... 24) instead of
            // always starting at day 1.
            var day = (periodStartDay - 1 + offset) % PeriodDays + 1;

            var dayIncomes = incomes
                .Where(i => i.RecurrenceDay == day)
                .Select(i => new BalanceEntryVm(i.Name, i.CategoryName, i.Amount))
                .ToList();
            var dayExpenses = expenses
                .Where(e => e.RecurrenceDay == day)
                .Select(e => new BalanceEntryVm(e.Name, e.CategoryName, e.Amount))
                .ToList();
            var daySavings = savings
                .Where(s => s.RecurrenceDay == day)
                .Select(s => new BalanceEntryVm(s.Name, s.CategoryName, s.Amount))
                .ToList();

            balance += dayIncomes.Sum(i => i.Amount) - dayExpenses.Sum(e => e.Amount) - daySavings.Sum(s => s.Amount);
            points.Add(new BalanceForecastPointVm(day, balance, dayIncomes, dayExpenses, daySavings));
        }

        return Response<List<BalanceForecastPointVm>>.Success(points);
    }
}
