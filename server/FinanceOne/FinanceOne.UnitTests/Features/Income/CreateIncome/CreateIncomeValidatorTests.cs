using FinanceOne.Api.Features.Income.CreateIncome;
using FluentValidation.TestHelper;

namespace FinanceOne.UnitTests.Features.Income.CreateIncome;

public class CreateIncomeValidatorTests
{
    private readonly CreateIncomeValidator _validator = new();

    private static CreateIncomeCommand Valid() => new("Monthly Salary", 45_000m, Guid.NewGuid(), 25);

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

    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    public void RecurrenceDay_Must_Be_Between_1_And_28(int day)
    {
        var result = _validator.TestValidate(Valid() with { RecurrenceDay = day });

        result.ShouldHaveValidationErrorFor(c => c.RecurrenceDay);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(28)]
    public void Valid_Command_Passes(int day)
    {
        var result = _validator.TestValidate(Valid() with { RecurrenceDay = day });

        result.ShouldNotHaveAnyValidationErrors();
    }
}
