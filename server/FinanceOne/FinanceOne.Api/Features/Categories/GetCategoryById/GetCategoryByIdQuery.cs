namespace FinanceOne.Api.Features.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<Response<CategoryVm>>;
