using FinanceOne.Api.Features.DiscountCodes.CreateDiscountCode;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace FinanceOne.UnitTests.Features.DiscountCodes.CreateDiscountCode;

public class CreateDiscountCodeValidatorTests
{
    private static readonly DateOnly Today = new(2026, 6, 15);

    private readonly CreateDiscountCodeValidator _validator =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero)));

    private static CreateDiscountCodeCommand Valid() => new("Rema 1000", "SAVE20", Today.AddDays(30));

    [Fact]
    public void StoreName_Is_Required()
    {
        var result = _validator.TestValidate(Valid() with { StoreName = "" });

        result.ShouldHaveValidationErrorFor(c => c.StoreName);
    }

    [Fact]
    public void ExpiryDate_Cannot_Be_In_The_Past()
    {
        var result = _validator.TestValidate(Valid() with { ExpiryDate = Today.AddDays(-1) });

        result.ShouldHaveValidationErrorFor(c => c.ExpiryDate)
            .WithErrorMessage("Expiry date cannot be in the past.");
    }

    // A code expiring today is still usable today, so the boundary is inclusive.
    [Fact]
    public void ExpiryDate_May_Be_Today()
    {
        var result = _validator.TestValidate(Valid() with { ExpiryDate = Today });

        result.ShouldNotHaveValidationErrorFor(c => c.ExpiryDate);
    }

    // A code can be a scannable image (added later via Upload Discount Code Image) instead of
    // text, so CodeText is not required on its own.
    [Fact]
    public void CodeText_Is_Optional()
    {
        var result = _validator.TestValidate(Valid() with { CodeText = null });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
