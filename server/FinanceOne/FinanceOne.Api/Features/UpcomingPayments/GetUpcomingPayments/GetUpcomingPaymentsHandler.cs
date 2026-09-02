namespace FinanceOne.Api.Features.UpcomingPayments.GetUpcomingPayments;

public sealed class GetUpcomingPaymentsHandler(IGetUpcomingPaymentsRepository repository, TimeProvider timeProvider)
    : IRequestHandler<GetUpcomingPaymentsQuery, Response<List<UpcomingPaymentVm>>>
{
    public async Task<Response<List<UpcomingPaymentVm>>> Handle(GetUpcomingPaymentsQuery request, CancellationToken cancellationToken)
    {
        var days = request.Days is > 0 ? request.Days.Value : 7;
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);

        var incomes = await repository.GetRecurringIncomes(cancellationToken);
        var expenses = await repository.GetRecurringExpenses(cancellationToken);

        var payments = new List<UpcomingPaymentVm>();
        for (var offset = 0; offset < days; offset++)
        {
            var date = today.AddDays(offset);

            payments.AddRange(incomes
                .Where(i => i.RecurrenceDay == date.Day)
                .Select(i => new UpcomingPaymentVm(date, i.Name, i.Amount, CategoryType.Income)));

            payments.AddRange(expenses
                .Where(e => e.RecurrenceDay == date.Day)
                .Select(e => new UpcomingPaymentVm(date, e.Name, e.Amount, CategoryType.Expense)));
        }

        return Response<List<UpcomingPaymentVm>>.Success(payments.OrderBy(p => p.Date).ToList());
    }
}
