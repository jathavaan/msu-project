using FinanceOne.Api.Features.Categories.UpdateCategory;
using FluentValidation.TestHelper;

namespace FinanceOne.UnitTests.Features.Categories.UpdateCategory;

public class UpdateCategoryValidatorTests
{
    private readonly UpdateCategoryValidator _validator = new();

    [Fact]
    public void Id_Is_Required()
    {
        var result = _validator.TestValidate(new UpdateCategoryCommand(Guid.Empty, "Rent", CategoryType.Expense));

        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Name_Is_Required()
    {
        var result = _validator.TestValidate(new UpdateCategoryCommand(Guid.NewGuid(), "", CategoryType.Expense));

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Type_Must_Be_A_Declared_CategoryType()
    {
        var result = _validator.TestValidate(new UpdateCategoryCommand(Guid.NewGuid(), "Rent", (CategoryType)42));

        result.ShouldHaveValidationErrorFor(c => c.Type);
    }

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(new UpdateCategoryCommand(Guid.NewGuid(), "Rent", CategoryType.Expense));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
