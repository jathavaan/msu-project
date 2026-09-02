using FinanceOne.Api.Domain.Enums;

namespace FinanceOne.Api.Domain.Entites;

public sealed class Category
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required CategoryType Type { get; set; }

    public ICollection<Income> Incomes { get; init; } = [];
    public ICollection<Expense> Expenses { get; init; } = [];
    public Budget? Budget { get; init; }
}
