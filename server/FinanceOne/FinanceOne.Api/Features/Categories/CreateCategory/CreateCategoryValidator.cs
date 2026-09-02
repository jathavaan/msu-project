using FluentValidation;

namespace FinanceOne.Api.Features.Categories.CreateCategory;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Type).IsInEnum();
    }
}
