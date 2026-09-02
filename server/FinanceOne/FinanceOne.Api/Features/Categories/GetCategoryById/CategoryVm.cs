namespace FinanceOne.Api.Features.Categories.GetCategoryById;

public sealed record CategoryVm(Guid Id, string Name, CategoryType Type);
