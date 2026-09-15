using FinanceOne.Api.Features.MonthlySavings.CreateMonthlySaving;
using FluentValidation.TestHelper;

namespace FinanceOne.UnitTests.Features.MonthlySavings.CreateMonthlySaving;

public class CreateMonthlySavingValidatorTests
{
    private readonly CreateMonthlySavingValidator _validator = new();

    private static CreateMonthlySavingCommand Valid() => new("Car Fund", 5_000m, Guid.NewGuid(), 25);

    [Fact]
    public void Name_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { Name = "" });

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Amount_Must_Be_Greater_Than_Zero(decimal amount)
    {
        var result = _validator.TestValidate(Valid() with { Amount = amount });

        result.ShouldHaveValidationErrorFor(c => c.Amount);
    }

    [Fact]
    public void SavingGoalId_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { SavingGoalId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(c => c.SavingGoalId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    public void RecurrenceDay_Must_Be_Between_1_And_28(int day)
    {
        var result = _validator.TestValidate(Valid() with { RecurrenceDay = day });

        result.ShouldHaveValidationErrorFor(c => c.RecurrenceDay);
    }

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
