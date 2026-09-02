using FluentValidation;

namespace FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;

public sealed class CreateSavingGoalValidator : AbstractValidator<CreateSavingGoalCommand>
{
    public CreateSavingGoalValidator(TimeProvider timeProvider)
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.TargetAmount).GreaterThan(0);
        RuleFor(c => c.TargetDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime))
            .WithMessage("Target date cannot be in the past.");
    }
}
