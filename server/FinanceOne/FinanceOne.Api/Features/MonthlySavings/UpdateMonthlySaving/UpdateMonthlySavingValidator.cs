using FluentValidation;

namespace FinanceOne.Api.Features.MonthlySavings.UpdateMonthlySaving;

public sealed class UpdateMonthlySavingValidator : AbstractValidator<UpdateMonthlySavingCommand>
{
    public UpdateMonthlySavingValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.SavingGoalId).NotEmpty();
        RuleFor(c => c.RecurrenceDay).InclusiveBetween(1, 28);
    }
}
