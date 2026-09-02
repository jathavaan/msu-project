namespace FinanceOne.Api.Features.Income.GetIncomeById;

public sealed record GetIncomeByIdQuery(Guid Id) : IRequest<Response<IncomeVm>>;
