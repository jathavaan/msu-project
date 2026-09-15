using FinanceOne.Api.Features.Categories.CreateCategory;
using FluentValidation.TestHelper;

namespace FinanceOne.UnitTests.Features.Categories.CreateCategory;

public class CreateCategoryValidatorTests
{
    private readonly CreateCategoryValidator _validator = new();

    [Fact]
    public void Name_Is_Required()
    {
        var result = _validator.TestValidate(new CreateCategoryCommand("", CategoryType.Expense));

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    // CategoryType is serialized as a number, so an out-of-range int deserializes into the enum
    // without error — IsInEnum is the only thing stopping it reaching the database.
    [Fact]
    public void Type_Must_Be_A_Declared_CategoryType()
    {
        var result = _validator.TestValidate(new CreateCategoryCommand("Rent", (CategoryType)42));

        result.ShouldHaveValidationErrorFor(c => c.Type);
    }

    [Theory]
    [InlineData(CategoryType.Income)]
    [InlineData(CategoryType.Expense)]
    public void Valid_Command_Passes(CategoryType type)
    {
        var result = _validator.TestValidate(new CreateCategoryCommand("Rent", type));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
