using FluentValidation;

namespace FinanceOne.Api.Features.CategorizationRules.CreateCategorizationRule;

public sealed class CreateCategorizationRuleValidator : AbstractValidator<CreateCategorizationRuleCommand>
{
    public CreateCategorizationRuleValidator()
    {
        RuleFor(c => c.Keyword).NotEmpty();
        RuleFor(c => c.CategoryId).NotEmpty();
    }
}
