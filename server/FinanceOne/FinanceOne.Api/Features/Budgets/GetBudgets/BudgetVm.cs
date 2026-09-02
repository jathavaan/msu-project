namespace FinanceOne.Api.Features.Budgets.GetBudgets;

public sealed record BudgetVm(Guid Id, Guid CategoryId, string CategoryName, decimal MonthlyLimit, decimal UsedThisMonth);
