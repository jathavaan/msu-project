using FinanceOne.Api.Features.SavingGoals.CreateSavingGoal;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.SavingGoals.CreateSavingGoal;

public class CreateSavingGoalValidatorTests
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    // The "not in the past" rule reads the clock, so it needs a fixed one — otherwise the test
    // would be comparing against whatever day CI happens to run on.
    private readonly CreateSavingGoalValidator _validator =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero)));

    private static CreateSavingGoalCommand Valid() => new("New Car", 250_000m, Today.AddMonths(12));

    [Fact]
    public void Name_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { Name = "" });

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TargetAmount_Must_Be_Greater_Than_Zero(decimal amount)
    {
        var result = _validator.TestValidate(Valid() with { TargetAmount = amount });

        result.ShouldHaveValidationErrorFor(c => c.TargetAmount);
    }

    [Fact]
    public void TargetDate_Cannot_Be_In_The_Past()
    {
        var result = _validator.TestValidate(Valid() with { TargetDate = Today.AddDays(-1) });

        result.ShouldHaveValidationErrorFor(c => c.TargetDate)
            .WithErrorMessage("Target date cannot be in the past.");
    }

    // The rule is GreaterThanOrEqualTo, so a goal due today is still valid.
    [Fact]
    public void TargetDate_May_Be_Today()
    {
        var result = _validator.TestValidate(Valid() with { TargetDate = Today });

        result.ShouldNotHaveValidationErrorFor(c => c.TargetDate);
    }

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
