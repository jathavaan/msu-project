using FluentValidation;

namespace FinanceOne.Api.Features.Expenses.UpdateExpense;

public sealed class UpdateExpenseValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.RecurrenceDay).InclusiveBetween(1, 28);
    }
}
