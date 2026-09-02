namespace FinanceOne.Api.Features.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Response<Unit>>;
