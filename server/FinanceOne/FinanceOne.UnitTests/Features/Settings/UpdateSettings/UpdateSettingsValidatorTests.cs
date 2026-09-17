using FinanceOne.Api.Features.Settings.UpdateSettings;
using FluentValidation.TestHelper;

namespace FinanceOne.UnitTests.Features.Settings.UpdateSettings;

public class UpdateSettingsValidatorTests
{
    private readonly UpdateSettingsValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    public void PeriodStartDay_Must_Be_Between_One_And_Twenty_Eight(int day)
    {
        var result = _validator.TestValidate(new UpdateSettingsCommand(day));

        result.ShouldHaveValidationErrorFor(c => c.PeriodStartDay);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(28)]
    public void PeriodStartDay_In_Range_Is_Valid(int day)
    {
        var result = _validator.TestValidate(new UpdateSettingsCommand(day));

        result.ShouldNotHaveValidationErrorFor(c => c.PeriodStartDay);
    }

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(new UpdateSettingsCommand(25));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
