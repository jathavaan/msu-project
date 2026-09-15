using FinanceOne.Api.Features.Budgets.CreateBudget;
using FluentValidation.TestHelper;

namespace FinanceOne.UnitTests.Features.Budgets.CreateBudget;

public class CreateBudgetValidatorTests
{
    private readonly CreateBudgetValidator _validator = new();

    [Fact]
    public void CategoryId_Is_Required()
    {
        var result = _validator.TestValidate(new CreateBudgetCommand(Guid.Empty, 1_000m));

        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void MonthlyLimit_Must_Be_Greater_Than_Zero(decimal monthlyLimit)
    {
        var result = _validator.TestValidate(new CreateBudgetCommand(Guid.NewGuid(), monthlyLimit));

        result.ShouldHaveValidationErrorFor(c => c.MonthlyLimit);
    }

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(new CreateBudgetCommand(Guid.NewGuid(), 1_000m));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
