using FinanceOne.Api.Domain.Enums;

namespace FinanceOne.Api.Features.SpendTrends.GetCategorySpendTrend;

public sealed class GetCategorySpendTrendHandler(IGetCategorySpendTrendRepository repository, TimeProvider timeProvider)
    : IRequestHandler<GetCategorySpendTrendQuery, Response<CategorySpendTrendVm>>
{
    // Fixed 6-month window per issue #49 — a user-selectable range was considered and deferred.
    private const int MonthsInTrend = 6;

    public async Task<Response<CategorySpendTrendVm>> Handle(GetCategorySpendTrendQuery request, CancellationToken cancellationToken)
    {
        var (name, type, monthlyLimit) = await repository.GetCategory(request.CategoryId, cancellationToken);
        if (name is null || type != CategoryType.Expense)
        {
            return Response<CategorySpendTrendVm>.Failure(StatusCodes.Status404NotFound, "Expense category not found.");
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        var currentMonthStart = new DateOnly(today.Year, today.Month, 1);
        var earliestMonthStart = currentMonthStart.AddMonths(-(MonthsInTrend - 1));

        var totals = await repository.GetMonthlySpend(request.CategoryId, earliestMonthStart, cancellationToken);

        var months = Enumerable.Range(0, MonthsInTrend)
            .Select(offset => earliestMonthStart.AddMonths(offset))
            .Select(monthStart =>
            {
                var actual = totals.FirstOrDefault(t => t.Year == monthStart.Year && t.Month == monthStart.Month).Total;
                return new MonthlySpendVm(monthStart.Year, monthStart.Month, actual);
            })
            .ToList();

        return Response<CategorySpendTrendVm>.Success(new CategorySpendTrendVm(request.CategoryId, name, monthlyLimit, months));
    }
}
