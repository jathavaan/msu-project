using FinanceOne.Api.Features.SavingGoals.UpdateSavingGoal;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.SavingGoals.UpdateSavingGoal;

public class UpdateSavingGoalValidatorTests
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly UpdateSavingGoalValidator _validator =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero)));

    private static UpdateSavingGoalCommand Valid() =>
        new(Guid.NewGuid(), "New Car", 250_000m, Today.AddMonths(12), 80_000m);

    [Fact]
    public void Id_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { Id = Guid.Empty });

        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Name_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { Name = "" });

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void TargetAmount_Must_Be_Greater_Than_Zero()
    {
        var result = _validator.TestValidate(Valid() with { TargetAmount = 0m });

        result.ShouldHaveValidationErrorFor(c => c.TargetAmount);
    }

    [Fact]
    public void TargetDate_Cannot_Be_In_The_Past()
    {
        var result = _validator.TestValidate(Valid() with { TargetDate = Today.AddDays(-1) });

        result.ShouldHaveValidationErrorFor(c => c.TargetDate)
            .WithErrorMessage("Target date cannot be in the past.");
    }

    // Unlike TargetAmount this one is GreaterThanOrEqualTo — a goal you have not put anything
    // towards yet is legitimate, a negative balance is not.
    [Fact]
    public void CurrentAmount_May_Be_Zero_But_Not_Negative()
    {
        _validator.TestValidate(Valid() with { CurrentAmount = 0m })
            .ShouldNotHaveValidationErrorFor(c => c.CurrentAmount);

        _validator.TestValidate(Valid() with { CurrentAmount = -1m })
            .ShouldHaveValidationErrorFor(c => c.CurrentAmount);
    }

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
