using FluentValidation;

namespace FinanceOne.Api.Features.Income.UpdateIncome;

public sealed class UpdateIncomeValidator : AbstractValidator<UpdateIncomeCommand>
{
    public UpdateIncomeValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.RecurrenceDay).InclusiveBetween(1, 28);
    }
}
