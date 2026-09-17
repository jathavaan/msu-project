using FluentValidation;

namespace FinanceOne.Api.Features.Settings.UpdateSettings;

public sealed class UpdateSettingsValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsValidator()
    {
        // Same 1-28 cap as Income/Expense/MonthlySaving.RecurrenceDay, so the balance forecast can
        // always walk a fixed 28-day period starting from this day regardless of the month.
        RuleFor(c => c.PeriodStartDay).InclusiveBetween(1, 28);
    }
}
