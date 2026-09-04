namespace FinanceOne.Api.Features.BalanceForecast.GetBalanceForecast;

public sealed class GetBalanceForecastHandler(IGetBalanceForecastRepository repository)
    : IRequestHandler<GetBalanceForecastQuery, Response<List<BalanceForecastPointVm>>>
{
    // Recurrence is day-of-month (1-28, see Income.RecurrenceDay/Expense.RecurrenceDay), so every
    // month can be walked as this same fixed 28-day period regardless of which month it actually is.
    private const int PeriodDays = 28;

    public async Task<Response<List<BalanceForecastPointVm>>> Handle(GetBalanceForecastQuery request, CancellationToken cancellationToken)
    {
        var incomes = await repository.GetRecurringIncomes(cancellationToken);
        var expenses = await repository.GetRecurringExpenses(cancellationToken);

        var points = new List<BalanceForecastPointVm>(PeriodDays);
        var balance = 0m;

        for (var day = 1; day <= PeriodDays; day++)
        {
            var dayIncomes = incomes
                .Where(i => i.RecurrenceDay == day)
                .Select(i => new BalanceEntryVm(i.Name, i.CategoryName, i.Amount))
                .ToList();
            var dayExpenses = expenses
                .Where(e => e.RecurrenceDay == day)
                .Select(e => new BalanceEntryVm(e.Name, e.CategoryName, e.Amount))
                .ToList();

            balance += dayIncomes.Sum(i => i.Amount) - dayExpenses.Sum(e => e.Amount);
            points.Add(new BalanceForecastPointVm(day, balance, dayIncomes, dayExpenses));
        }

        return Response<List<BalanceForecastPointVm>>.Success(points);
    }
}
