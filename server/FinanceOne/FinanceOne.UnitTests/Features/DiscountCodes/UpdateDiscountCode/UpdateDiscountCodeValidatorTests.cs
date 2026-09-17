using FinanceOne.Api.Features.DiscountCodes.UpdateDiscountCode;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.DiscountCodes.UpdateDiscountCode;

public class UpdateDiscountCodeValidatorTests
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly UpdateDiscountCodeValidator _validator =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero)));

    private static UpdateDiscountCodeCommand Valid() =>
        new(Guid.NewGuid(), "Rema 1000", "SAVE20", Today.AddDays(30));

    [Fact]
    public void Id_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { Id = Guid.Empty });

        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void StoreName_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { StoreName = "" });

        result.ShouldHaveValidationErrorFor(c => c.StoreName);
    }

    // Worth knowing: because the rule is applied on update too, an already-expired code cannot be
    // edited without also pushing its expiry date forward.
    [Fact]
    public void ExpiryDate_Cannot_Be_In_The_Past()
    {
        var result = _validator.TestValidate(Valid() with { ExpiryDate = Today.AddDays(-1) });

        result.ShouldHaveValidationErrorFor(c => c.ExpiryDate)
            .WithErrorMessage("Expiry date cannot be in the past.");
    }

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
