namespace FinanceOne.Api.Features.Income.GetIncomes;

public sealed record GetIncomesQuery : IRequest<Response<List<IncomeVm>>>;
