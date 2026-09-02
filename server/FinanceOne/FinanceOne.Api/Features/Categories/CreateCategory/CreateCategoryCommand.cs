namespace FinanceOne.Api.Features.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name, CategoryType Type) : IRequest<Response<Guid>>;
