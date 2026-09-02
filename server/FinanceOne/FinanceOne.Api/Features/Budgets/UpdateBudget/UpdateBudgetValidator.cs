using FluentValidation;

namespace FinanceOne.Api.Features.Budgets.UpdateBudget;

public sealed class UpdateBudgetValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.MonthlyLimit).GreaterThan(0);
    }
}
