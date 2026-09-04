using FluentValidation;

namespace FinanceOne.Api.Features.MonthlySavings.CreateMonthlySaving;

public sealed class CreateMonthlySavingValidator : AbstractValidator<CreateMonthlySavingCommand>
{
    public CreateMonthlySavingValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.SavingGoalId).NotEmpty();
        RuleFor(c => c.RecurrenceDay).InclusiveBetween(1, 28);
    }
}
