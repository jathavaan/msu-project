using FinanceOne.Api.Features.Expenses.CreateExpense;
using FluentValidation.TestHelper;

namespace FinanceOne.UnitTests.Features.Expenses.CreateExpense;

public class CreateExpenseValidatorTests
{
    private readonly CreateExpenseValidator _validator = new();

    private static CreateExpenseCommand Valid() => new("Rent", 12_000m, Guid.NewGuid(), 1);

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
    public void CategoryId_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { CategoryId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    // Capped at 28 so a recurring day exists in every month, February included.
    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    [InlineData(31)]
    [InlineData(-1)]
    public void RecurrenceDay_Must_Be_Between_1_And_28(int day)
    {
        var result = _validator.TestValidate(Valid() with { RecurrenceDay = day });

        result.ShouldHaveValidationErrorFor(c => c.RecurrenceDay);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(28)]
    public void Valid_Command_Passes(int day)
    {
        var result = _validator.TestValidate(Valid() with { RecurrenceDay = day });

        result.ShouldNotHaveAnyValidationErrors();
    }
}
