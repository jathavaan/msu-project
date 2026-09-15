using FinanceOne.Api.Features.Expenses.UpdateExpense;
using FluentValidation.TestHelper;

namespace FinanceOne.UnitTests.Features.Expenses.UpdateExpense;

public class UpdateExpenseValidatorTests
{
    private readonly UpdateExpenseValidator _validator = new();

    private static UpdateExpenseCommand Valid() => new(Guid.NewGuid(), "Rent", 12_000m, Guid.NewGuid(), 1);

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
    public void Amount_Must_Be_Greater_Than_Zero()
    {
        var result = _validator.TestValidate(Valid() with { Amount = 0m });

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

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
