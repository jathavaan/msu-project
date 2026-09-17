using FinanceOne.Api.Features.CategorizationRules.CreateCategorizationRule;
using FluentValidation.TestHelper;

namespace FinanceOne.UnitTests.Features.CategorizationRules.CreateCategorizationRule;

public class CreateCategorizationRuleValidatorTests
{
    private readonly CreateCategorizationRuleValidator _validator = new();

    private static CreateCategorizationRuleCommand Valid() => new("REMA", Guid.NewGuid());

    [Fact]
    public void Keyword_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { Keyword = "" });

        result.ShouldHaveValidationErrorFor(c => c.Keyword);
    }

    [Fact]
    public void CategoryId_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { CategoryId = Guid.Empty });

        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
