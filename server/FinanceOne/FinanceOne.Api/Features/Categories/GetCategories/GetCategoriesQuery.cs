namespace FinanceOne.Api.Features.Categories.GetCategories;

public sealed record GetCategoriesQuery(CategoryType? Type) : IRequest<Response<List<CategoryVm>>>;
