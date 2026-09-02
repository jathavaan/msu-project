namespace FinanceOne.Api.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(Guid Id, string Name, CategoryType Type) : IRequest<Response<Unit>>;
