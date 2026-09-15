using FinanceOne.Api.Features.Budgets.UpdateBudget;
using FluentValidation.TestHelper;

namespace FinanceOne.UnitTests.Features.Budgets.UpdateBudget;

public class UpdateBudgetValidatorTests
{
    private readonly UpdateBudgetValidator _validator = new();

    [Fact]
    public void Id_Is_Required()
    {
        var result = _validator.TestValidate(new UpdateBudgetCommand(Guid.Empty, 1_000m));

        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void MonthlyLimit_Must_Be_Greater_Than_Zero(decimal monthlyLimit)
    {
        var result = _validator.TestValidate(new UpdateBudgetCommand(Guid.NewGuid(), monthlyLimit));

        result.ShouldHaveValidationErrorFor(c => c.MonthlyLimit);
    }

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(new UpdateBudgetCommand(Guid.NewGuid(), 1_000m));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
