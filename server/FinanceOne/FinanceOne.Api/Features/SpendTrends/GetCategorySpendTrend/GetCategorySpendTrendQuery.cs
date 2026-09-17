namespace FinanceOne.Api.Features.SpendTrends.GetCategorySpendTrend;

public sealed record GetCategorySpendTrendQuery(Guid CategoryId) : IRequest<Response<CategorySpendTrendVm>>;
