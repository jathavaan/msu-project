using FluentValidation;

namespace FinanceOne.Api.Features.Income.CreateIncome;

public sealed class CreateIncomeValidator : AbstractValidator<CreateIncomeCommand>
{
    public CreateIncomeValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.RecurrenceDay).InclusiveBetween(1, 28);
    }
}
