using FluentValidation;

namespace FinanceOne.Api.Features.SavingGoals.UpdateSavingGoal;

public sealed class UpdateSavingGoalValidator : AbstractValidator<UpdateSavingGoalCommand>
{
    public UpdateSavingGoalValidator(TimeProvider timeProvider)
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.TargetAmount).GreaterThan(0);
        RuleFor(c => c.TargetDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime))
            .WithMessage("Target date cannot be in the past.");
        RuleFor(c => c.CurrentAmount).GreaterThanOrEqualTo(0);
    }
}
