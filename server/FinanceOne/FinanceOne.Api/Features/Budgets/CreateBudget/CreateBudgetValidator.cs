using FluentValidation;

namespace FinanceOne.Api.Features.Budgets.CreateBudget;

public sealed class CreateBudgetValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetValidator()
    {
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.MonthlyLimit).GreaterThan(0);
    }
}
